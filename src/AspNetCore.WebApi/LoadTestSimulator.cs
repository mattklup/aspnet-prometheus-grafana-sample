using System;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCore.Abstractions.Observability;

namespace AspNetCore.Controllers
{
    internal class LoadTestSimulator : ILoadTestSimulator
    {
        private readonly ICoreMetrics metrics;
        private readonly ICoreTelemetry telemetry;
        private readonly IHttpClientFactory httpClientFactory;

        public LoadTestSimulator(ICoreMetrics metrics, ICoreTelemetry telemetry, IHttpClientFactory httpClientFactory)
        {
            this.metrics = metrics;
            this.telemetry = telemetry;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> SimulateLoadTestAsync()
        {
            Random random = new Random();
            string user = $"user-{random.Next(10)}";
            using var httpClient = this.httpClientFactory.CreateClient();

            using var span = this.telemetry.Start("LoadTest");

            span.SetTag("user", user);

            if (random.Next(10) == 0)
            {
                var value = random.Next(5);
                var exception = value switch
                {
                    0 => new InvalidOperationException("load test"),
                    _ when value <= 3 => new NotSupportedException("load test"),
                    _ => new Exception("Unknown"),
                };

                throw exception;
            }

            // simulate work
            await Task.Delay(TimeSpan.FromSeconds(5));

            return user;
        }
    }
}