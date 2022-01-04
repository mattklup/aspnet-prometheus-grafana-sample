using System;

namespace AspNetCore.Abstractions.Observability
{
    public interface ICoreTelemetrySpan : IDisposable
    {
        void SetTag(string key, object value);

        void SetBaggage(string key, string value);
    }
}
