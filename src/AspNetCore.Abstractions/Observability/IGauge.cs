using System.Collections.Generic;

namespace AspNetCore.Abstractions.Observability
{
    public interface IGauge<T>
      where T : GaugeData
    {
        void SetCount(double count);
    }

    public abstract class GaugeData
    {
        public string Name { get; }

        public string Help { get; }

        public IEnumerable<string> Labels { get; }
    }
}
