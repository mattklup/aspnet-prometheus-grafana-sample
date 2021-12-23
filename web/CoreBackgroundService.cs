using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace AspNetCore
{
    class CoreBackgroundService : BackgroundService
    {
        private readonly ICoreMetrics metrics;

        public CoreBackgroundService(ICoreMetrics metrics)
        {
            this.metrics = metrics;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Random random = new();
            double workloadCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                workloadCount += random.Next(-10, 10);
                workloadCount = Math.Max(0, workloadCount);
                workloadCount = Math.Min(100, workloadCount);

                this.metrics.ActiveWorkloads(workloadCount);

                await Task.Delay(TimeSpan.FromSeconds(random.Next(5, 10)), cancellationToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}