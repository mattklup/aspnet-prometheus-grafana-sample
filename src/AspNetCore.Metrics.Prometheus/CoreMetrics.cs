using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using AspNetCore.Abstractions.Observability;

namespace AspNetCore.Observability
{
    class CoreMetrics : ICoreMetrics
    {
        static readonly Gauge ApplicationInfoCounter = Metrics.CreateGauge(
            "app_info",
            "Basic application runtime information",
            "version", "description");

        private readonly Counter totalRequests = Metrics.CreateCounter(
            "sample_total_requests",
            "The total number of requests serviced.",
            "method");

        private readonly Counter totalExceptions = Metrics.CreateCounter(
            "sample_total_exceptions",
            "The total number of requests serviced.",
            "exception_type");

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

        public void OnException(Exception exception)
        {
            this.totalExceptions
                .WithLabels(exception.GetType().Name)
                .Inc();
        }

        public void ActiveWorkloads(double workloadCount)
        {
            this.workloadGauge.Set(workloadCount);
        }
    }
}