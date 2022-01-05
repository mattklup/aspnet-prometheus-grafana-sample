using System;
using System.Threading.Tasks;
using AspNetCore.Abstractions.Observability;
using AspNetCore.Observability;
using AspNetCore.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Prometheus;

namespace AspNetCoreBackend
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
                    services.AddCoreMetrics(services.AddHealthChecks());
                    services.AddOpenTelemetry("SampleServiceBackend", "1.0.0");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(applicationBuilder =>
                    {
                        applicationBuilder.UseRouting();
                        applicationBuilder.UseCoreMetricsMiddleware();
                        applicationBuilder.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHealthChecks("/healthcheck");
                            endpoints.MapMetrics();

                            endpoints.Map("/getuser", async (context) =>
                            {
                                var telemetry = applicationBuilder.ApplicationServices.GetRequiredService<ICoreTelemetry>();
                                using var span = telemetry.Start("GetUser");

                                try
                                {
                                    var id = Guid.NewGuid().ToString();

                                    await Task.Delay(TimeSpan.FromSeconds(1));
                                    span?.SetTag("Message", id);

                                    await RabbitMQ.QueueAsync(id);
                                    await Task.Delay(TimeSpan.FromSeconds(1));

                                    context.Response.StatusCode = 200;
                                    await context.Response.WriteAsync(id);
                                }
                                catch (Exception exception)
                                {
                                    span?.SetTag("exception-type", exception.GetType().Name);
                                    span?.SetTag("exception-message", exception.Message);
                                }
                            });
                        });

                        applicationBuilder.Use(async (context, next) =>
                        {
                            var telemetry = applicationBuilder.ApplicationServices.GetRequiredService<ICoreTelemetry>();
                            using var span = telemetry.Start("QueueMessage");

                            try
                            {
                                await RabbitMQ.QueueAsync();
                                await context.Response.WriteAsync($"hi, you wanted '{context.Request.Path}'");
                            }
                            catch (Exception exception)
                            {
                                span?.SetTag("exception-type", exception.GetType().Name);
                                span?.SetTag("exception-message", exception.Message);
                            }
                        });
                    });
                });
    }
}
