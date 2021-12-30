using System;
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

            services.AddSingleton<ICoreTelemetry, CoreTelemetry>();

            return services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    //.AddConsoleExporter()
                    //.AddSource(serviceName, "OpenTelemetry.Instrumentation.AspNetCore")
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

    public interface ICoreTelemetry
    {
        ICoreTelemetrySpan Start(string name);
    }

    public interface ICoreTelemetrySpan : IDisposable
    {
        void SetTag(string key, object value);
    }

    class CoreTelemetry : ICoreTelemetry
    {
        private static readonly ActivitySource source = new ActivitySource(nameof(CoreTelemetry));

        public ICoreTelemetrySpan Start(string name)
        {
            return new Span()
            {
                Activity = source.StartActivity(name)
            };
        }

        internal class Span : ICoreTelemetrySpan
        {
            public Activity Activity { get; init; }

            public void SetTag(string key, object value)
            {
                this.Activity?.SetTag(key, value);
            }

            public void Dispose()
            {
                this.Activity?.Dispose();
            }
        }
    }
}