using System;
using System.Threading.Tasks;
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
                .ConfigureServices(services => {
                    services.AddHealthChecks()
                        .ForwardToPrometheus();
                    services.AddOpenTelemetry();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(applicationBuilder =>
                    {
                        applicationBuilder.UseRouting();
                        applicationBuilder.UseHttpMetrics();
                        applicationBuilder.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHealthChecks("/healthcheck");
                            endpoints.MapMetrics();

                            endpoints.Map("/getuser", async (context) => {
                                var telemetry = applicationBuilder.ApplicationServices.GetRequiredService<ICoreTelemetry>();
                                using var span = telemetry.Start("GetUser");

                                await Task.Delay(TimeSpan.FromSeconds(2));

                                context.Response.StatusCode = 200;
                                await context.Response.WriteAsync(Guid.NewGuid().ToString());
                            });
                        });

                        applicationBuilder.Use(async (context, next) =>
                        {
                            await context.Response.WriteAsync($"hi, you wanted '{context.Request.Path}'");
                        });
                    });
                });
    }
}