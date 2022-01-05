using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using AspNetCore.Abstractions.Observability;

namespace AspNetCore.Observability
{
    public static class CoreMetricsExtensions
    {
        public static IServiceCollection AddCoreMetrics(this IServiceCollection services, IHealthChecksBuilder healthChecksBuilder = null)
        {
            services.AddSingleton<ICoreMetrics, CoreMetrics>();
            healthChecksBuilder?.ForwardToPrometheus();

            return services;
        }

        public static IApplicationBuilder UseCoreMetricsMiddleware(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseHttpMetrics();
            applicationBuilder.UseMiddleware<ExceptionMetricsMiddleware>();

            return applicationBuilder;
        }
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

    public class ExceptionMetricsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ICoreMetrics metrics;

        public ExceptionMetricsMiddleware(RequestDelegate next, ICoreMetrics metrics)
        {
            this.next = next;
            this.metrics = metrics;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch(Exception excption)
            {
                this.metrics.OnException(excption);

                context.Response.StatusCode = 500;
                throw;
            }
        }
    }

}