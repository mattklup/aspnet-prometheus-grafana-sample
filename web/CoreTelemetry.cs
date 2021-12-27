using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

namespace AspNetCore
{
    static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services)
        {
            // Define some important constants and the activity source
            var serviceName = "SampleService";
            var serviceVersion = "1.0.0";

            return services.AddOpenTelemetryTracing(builder => 
            {
                builder
                    .AddConsoleExporter()
                    .AddSource(serviceName, "OpenTelemetry.Instrumentation.AspNetCore")
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

    interface ICoreTelemetry
    {
    }

    class CoreTelemetry : ICoreTelemetry
    {
        
    }
}