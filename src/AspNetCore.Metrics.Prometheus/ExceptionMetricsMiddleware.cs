using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AspNetCore.Abstractions.Observability;

namespace AspNetCore.Observability
{
    public class ExceptionMetricsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ICoreMetrics metrics;

        public ExceptionMetricsMiddleware(RequestDelegate next, ICoreMetrics metrics)
        {
            this.next = next;
            this.metrics = metrics;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch(Exception excption)
            {
                this.metrics.OnException(excption);

                context.Response.StatusCode = 500;
                throw;
            }
        }
    }
}