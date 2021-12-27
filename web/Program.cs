using System.Threading.Tasks;
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
                .ConfigureServices(services => {
                    services.AddControllers();
                    services.AddHealthChecks()
                        .ForwardToPrometheus();
                    services.AddSingleton<ICoreMetrics, CoreMetrics>();
                    services.AddHostedService<CoreBackgroundService>();
                    services.AddOpenTelemetry();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(applicationBuilder =>
                    {
                        var metrics = applicationBuilder.ApplicationServices.GetRequiredService<ICoreMetrics>();
                        metrics.ApplicationInfo();

                        applicationBuilder.UseRouting();
                        applicationBuilder.UseHttpMetrics();
                        applicationBuilder.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            endpoints.MapHealthChecks("/healthcheck");
                            endpoints.MapMetrics();
                        });

                        applicationBuilder.Use(async (context, next) =>
                        {
                            metrics.OnRequest(context.Request.Method);
                            
                            await context.Response.WriteAsync($"hi, you wanted '{context.Request.Path}'");
                        });
                    });
                });
    }
}
