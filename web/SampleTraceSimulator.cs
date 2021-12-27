using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace AspNetCore
{
    class SampleTraceSimulator
    {
        private readonly ICoreMetrics metrics;
        private readonly ICoreTelemetry telemetry;

        public SampleTraceSimulator(ICoreMetrics metrics, ICoreTelemetry telemetry)
        {
            this.metrics = metrics;
            this.telemetry = telemetry;
        }

        public async Task SimulateSampleTraceAsync()
        {
            using var span = this.telemetry.Start(nameof(SampleTraceSimulator));

            // simulate work
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}