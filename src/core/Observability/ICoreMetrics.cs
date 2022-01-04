using System;

namespace AspNetCore.Abstractions.Observability
{
    public interface ICoreMetrics
    {
        void ApplicationInfo();

        void OnRequest(string method);

        void OnException(Exception exception);

        void ActiveWorkloads(double workloadCount);
    }
}
