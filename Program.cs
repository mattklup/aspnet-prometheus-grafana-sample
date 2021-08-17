using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace aspnetcore
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
                    services.AddHealthChecks();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(applicationBuilder =>
                    {
                        applicationBuilder.UseRouting();

                        applicationBuilder.UseHttpMetrics();

                        applicationBuilder.UseEndpoints(endpoints =>
                        {
                            endpoints.MapMetrics();
                        });

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
