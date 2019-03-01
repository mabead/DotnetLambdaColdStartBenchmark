using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace AspnetCoreLambda
{
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        static LambdaEntryPoint()
        {
            Console.WriteLine($"{DateTime.UtcNow:o} : LambdaEntryPoint static constructor");
        }

        public LambdaEntryPoint()
        {
            Console.WriteLine($"{DateTime.UtcNow:o} : LambdaEntryPoint constructor");
        }

        protected override void Init(IWebHostBuilder builder)
        {
            Console.WriteLine($"{DateTime.UtcNow:o} : Start of Init");

            builder
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging
                        .ClearProviders()
                        .AddProvider(new CustomLoggerProvider());
                });

            Console.WriteLine($"{DateTime.UtcNow:o} : End of Init");
        }
    }
}
