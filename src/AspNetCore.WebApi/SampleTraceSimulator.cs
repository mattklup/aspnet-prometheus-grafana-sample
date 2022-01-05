using System;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCore.Abstractions.Observability;

namespace AspNetCore
{
    internal class SampleTraceSimulator
    {
        private readonly ICoreMetrics metrics;
        private readonly ICoreTelemetry telemetry;
        private readonly IHttpClientFactory httpClientFactory;

        public SampleTraceSimulator(ICoreMetrics metrics, ICoreTelemetry telemetry, IHttpClientFactory httpClientFactory)
        {
            this.metrics = metrics;
            this.telemetry = telemetry;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task SimulateSampleTraceAsync()
        {
            Random random = new Random();
            using var httpClient = this.httpClientFactory.CreateClient();

            using var span = this.telemetry.Start(nameof(SampleTraceSimulator));

            span?.SetBaggage("start-time", DateTimeOffset.UtcNow.ToString());

            // Call to other service on docker network
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://aspnetcorebackend-1/getuser")
                {
                    Headers =
                    {
                        { "x-test-header", "test-header" },
                    },
                };

            var task = httpClient.SendAsync(request)
                .ContinueWith(async (responseTask) =>
                {
                    var response = responseTask.Result;
                    response.EnsureSuccessStatusCode();
                    var user = await response.Content.ReadAsStringAsync();

                    span.SetTag("user", user);
                    return user;
                });

            // Sub span
            {
                using var subSpan1 = this.telemetry.Start(nameof(SampleTraceSimulator) + "_1");
                await Task.Delay(TimeSpan.FromSeconds(random.Next(2, 5)));
            }

            // Sub span
            {
                using var subSpan2 = this.telemetry.Start(nameof(SampleTraceSimulator) + "_2");
                await Task.Delay(TimeSpan.FromSeconds(random.Next(2, 5)));
            }

            {
                using var subSpan3 = this.telemetry.Start(nameof(SampleTraceSimulator) + "_3");
                await Task.Delay(TimeSpan.FromSeconds(random.Next(2, 5)));
            }

            await task;
        }
    }
}