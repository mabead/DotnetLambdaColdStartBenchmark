using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Benchmarks
{
    public class AspNetCoreLambdaVsSimpleLambda
    {
        private HttpClient _httpClient;
        private AmazonLambdaClient _lambdaClient;

        [GlobalSetup]
        public void Setup()
        {
            _httpClient = new HttpClient();
            // This is the URL of my ASP.NET core lambda. You will most likely want to chagne this.
            _httpClient.BaseAddress = new Uri("https://yeotwci6j1.execute-api.us-east-1.amazonaws.com");

            _lambdaClient = new AmazonLambdaClient(RegionEndpoint.USEast1);
        }

        [Benchmark]
        public async Task SimpleLambda()
        {
            var response = await _lambdaClient.InvokeAsync(new InvokeRequest
            {
                FunctionName = "SimpleFunction",
                InvocationType = InvocationType.RequestResponse,
                Payload = "{}",
            });
        }

        [Benchmark]
        public async Task AspnetCoreLambda()
        {
            var response = await _httpClient.GetAsync("Prod/api/values");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to invoke ASP.NET Core lambda: {response.RequestMessage.RequestUri}");
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
