using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using AspNetCore.Abstractions.Observability;

namespace AspNetCore.Telemetry
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services)
        {
            // Define some important constants and the activity source
            var serviceName = "SampleService";
            var serviceVersion = "1.0.0";

            services.AddSingleton<ICoreTelemetry, CoreTelemetry>();

            return services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    //.AddConsoleExporter()
                    .AddSource(serviceName, nameof(CoreTelemetry))
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