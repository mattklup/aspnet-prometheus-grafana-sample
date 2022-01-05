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
}