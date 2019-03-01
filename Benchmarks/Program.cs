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
    [SimpleJob(launchCount: 1, warmupCount: 3)]
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
