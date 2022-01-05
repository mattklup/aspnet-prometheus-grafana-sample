using AspNetCore.Abstractions.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

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