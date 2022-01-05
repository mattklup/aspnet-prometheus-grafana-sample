using System.Diagnostics;
using AspNetCore.Abstractions.Observability;

namespace AspNetCore.Telemetry
{
    internal class CoreTelemetry : ICoreTelemetry
    {
        private static readonly ActivitySource source = new ActivitySource(nameof(CoreTelemetry));

        public ICoreTelemetrySpan Start(string name)
        {
            return new Span()
            {
                Activity = source.StartActivity(name),
            };
        }

        internal class Span : ICoreTelemetrySpan
        {
            public Activity Activity { get; init; }

            public void SetTag(string key, object value)
            {
                this.Activity?.SetTag(key, value);
            }

            public void SetBaggage(string key, string value)
            {
                this.Activity?.SetBaggage(key, value);
            }

            public void Dispose()
            {
                this.Activity?.Dispose();
            }
        }
    }
}