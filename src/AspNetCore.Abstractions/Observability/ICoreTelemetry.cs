using System;

namespace AspNetCore.Abstractions.Observability
{
    public interface ICoreTelemetry
    {
        ICoreTelemetrySpan Start(string name);
    }
}
