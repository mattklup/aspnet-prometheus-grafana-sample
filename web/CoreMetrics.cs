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
    interface ICoreMetrics
    {
        void ApplicationInfo();

        void OnRequest(string method);

        void ActiveWorkloads(double workloadCount);
    }

    class CoreMetrics : ICoreMetrics
    {
        static readonly Gauge ApplicationInfoCounter = Metrics.CreateGauge(
                "app_info",
                "Basic application runtime information",
                "version", "description");

        private readonly Counter totalRequests = Metrics.CreateCounter(
            "sample_total_requests",
            "The total number of requests serviced.",
            new CounterConfiguration { LabelNames = new[] { "method" } });

        private readonly Gauge workloadGauge = Metrics.CreateGauge(
            "sample_workload_count",
            "Count of active workloads.");

        public void ApplicationInfo()
        {
            ApplicationInfoCounter
                .WithLabels(System.Environment.Version.ToString(), System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
                .Set(1);
        }

        public void OnRequest(string method)
        {
            this.totalRequests
                .WithLabels(method)
                .Inc();
        }

        public void ActiveWorkloads(double workloadCount)
        {
            this.workloadGauge.Set(workloadCount);
        }
    }
}