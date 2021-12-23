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
                    })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(applicationBuilder =>
                    {
                        applicationBuilder.UseRouting();
                        applicationBuilder.UseHttpMetrics();
                        applicationBuilder.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            endpoints.MapHealthChecks("/healthcheck");
                            endpoints.MapMetrics();
                        });

                        // Use counter one time for some application info
                        Metrics.CreateCounter(
                                "app_info",
                                "The total number of requests serviced.",
                                new CounterConfiguration { LabelNames = new[] { "version", "description" } })
                            .WithLabels(System.Environment.Version.ToString(), System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
                            .Inc();

                        var counter = Metrics.CreateCounter("sample_total_requests", "The total number of requests serviced.");

                        applicationBuilder.Use(async (context, next) =>
                        {
                            await context.Response.WriteAsync($"hi, you wanted '{context.Request.Path}'");

                            counter.Inc();
                        });
                    });
                });
    }
}
