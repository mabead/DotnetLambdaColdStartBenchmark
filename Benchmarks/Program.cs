using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Benchmarks
{
    public class AspNetCoreLambdaVsSimpleLambda
    {
        #region Deployment specific parameters

        private const string AspNetCoreRouteUrl = "https://yeotwci6j1.execute-api.us-east-1.amazonaws.com/Prod/api/values";
        private const string SimpleFunctionName = "SimpleFunction";
        private const string AspNetCoreFunctionName = "AspnetCoreLambda-AspNetCoreFunction-E6UTZD3QUFZP";
        private RegionEndpoint Region = RegionEndpoint.USEast1;

        #endregion

        private HttpClient _httpClient;
        private AmazonLambdaClient _lambdaClient;


        [GlobalSetup]
        public void GlobalSetup()
        {
            _httpClient = new HttpClient();
            _lambdaClient = new AmazonLambdaClient(Region);
        }

        private void ForceLambdaColdStart(string functionName)
        {
            var value = Guid.NewGuid().ToString().Replace("-", "");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "aws",
                Arguments = $"lambda update-function-configuration --function-name {functionName} --environment Variables={{foo={value}}}",
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception("Failed to change lambda configuration");
                }
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            ForceLambdaColdStart(AspNetCoreFunctionName);
            ForceLambdaColdStart(SimpleFunctionName);
        }

        [Benchmark]
        public async Task SimpleLambda()
        {
            var response = await _lambdaClient.InvokeAsync(new InvokeRequest
            {
                FunctionName = SimpleFunctionName,
                InvocationType = InvocationType.RequestResponse,
                Payload = "{}",
            });
        }

        [Benchmark]
        public async Task AspnetCoreLambda()
        {
            var response = await _httpClient.GetAsync(AspNetCoreRouteUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to invoke ASP.NET Core lambda");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AspNetCoreLambdaVsSimpleLambda>();
        }
    }
}
