using AspNetCore.Abstractions.Observability;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AspNetCore.Telemetry
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, string serviceName, string serviceVersion, params string[] sources)
        {
            services.AddSingleton<ICoreTelemetry, CoreTelemetry>();

            return services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .AddSource(serviceName, nameof(CoreTelemetry))
                    .AddSource(sources)
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddJaegerExporter(jaegerOptions =>
                        {
                            jaegerOptions.AgentHost = "jaeger"; // Use name from docker-compose file, not "localhost";
                            jaegerOptions.AgentPort = 6831;
                        });
            });
        }
    }
}