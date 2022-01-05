using System;
using System.Threading.Tasks;
using AspNetCore.Abstractions.Observability;
using AspNetCore.Controllers;
using AspNetCore.Observability;
using AspNetCore.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace AspNetCore
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddCoreMetrics(services.AddHealthChecks());
                    services.AddOpenTelemetry("SampleService", nameof(QueueBackgroundService));

                    services.AddHostedService<CoreBackgroundService>(); // Simulate batch work
                    services.AddHostedService<QueueBackgroundService>(); // Read queue messages

                    services.AddSingleton<SampleTraceSimulator>();
                    services.AddSingleton<ILoadTestSimulator, LoadTestSimulator>();

                    // Since I didn't use 'UseStartup', services.AddControllers() won't work by itself
                    services.AddControllers().AddApplicationPart(typeof(Program).Assembly).AddControllersAsServices();
                    services.AddSwaggerGen();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(applicationBuilder =>
                    {
                        var metrics = applicationBuilder.ApplicationServices.GetRequiredService<ICoreMetrics>();
                        metrics.ApplicationInfo();

                        applicationBuilder.UseSwagger();
                        applicationBuilder.UseSwaggerUI();

                        applicationBuilder.UseRouting();
                        applicationBuilder.UseCoreMetricsMiddleware();

                        applicationBuilder.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            endpoints.MapHealthChecks("/healthcheck");
                            endpoints.MapMetrics();

                            endpoints.Map("/trace", async (context) =>
                            {
                                var traceSimulator = applicationBuilder.ApplicationServices.GetRequiredService<SampleTraceSimulator>();

                                await traceSimulator.SimulateSampleTraceAsync();
                            });
                        });

                        applicationBuilder.Use(async (context, next) =>
                        {
                            metrics.OnRequest(context.Request.Method);

                            if (context.Request.Path.Value.Contains("error", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidOperationException("error");
                            }

                            await context.Response.WriteAsync($"hi, you wanted '{context.Request.Path}'");
                        });
                    });
                });
    }
}
