using System.Collections.Generic;

namespace JTLStudio.SDK
{
    public interface IAnalytics : IModule
    {
        void Report(string eventName);

        void Report(string eventName, IReadOnlyDictionary<string, object> parameters);
    }
}
