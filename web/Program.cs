using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AspNetCore.Controllers;
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
                .ConfigureServices(services => {
                    services.AddHttpClient();
                    services.AddCoreMetrics(services.AddHealthChecks());
                    services.AddOpenTelemetry();

                    services.AddHostedService<CoreBackgroundService>();
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

                        /* Use typed middleware instead
                        applicationBuilder.Use(async (context, next) =>
                        {
                            try
                            {
                                await next();
                            }
                            catch(Exception)
                            {
                                context.Response.StatusCode = 500;
                                throw;
                            }
                        });
                        */

                        applicationBuilder.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            endpoints.MapHealthChecks("/healthcheck");
                            endpoints.MapMetrics();

                            endpoints.Map("/trace", async (context) => {
                                var traceSimulator = applicationBuilder.ApplicationServices.GetRequiredService<SampleTraceSimulator>();

                                await traceSimulator.SimulateSampleTraceAsync();
                                //context.Response.StatusCode = 200;
                                //await context.Response.WriteAsync("Trace");
                            });
                        });

                        applicationBuilder.Use(async (context, next) =>
                        {
                            metrics.OnRequest(context.Request.Method);

                            if (context.Request.Path.Value.Contains("error", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidOperationException("error");
                            }

                            /*
                            var currentEndpoint = context.GetEndpoint();

                            if (currentEndpoint is null)
                            {
                                await next();
                                return;
                            }

                            Console.WriteLine($"Endpoint: {currentEndpoint.DisplayName}");

                            if (currentEndpoint is RouteEndpoint routeEndpoint)
                            {
                                Console.WriteLine($"  - Route Pattern: {routeEndpoint.RoutePattern}");
                            }

                            foreach (var endpointMetadata in currentEndpoint.Metadata)
                            {
                                Console.WriteLine($"  - Metadata: {endpointMetadata}");
                            }
                            */

                            await context.Response.WriteAsync($"hi, you wanted '{context.Request.Path}'");
                        });
                    });
                });
    }
}
