using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests.Fakes
{
    public class FakeFlagsProvider : IFlagsProvider
    {
        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>();

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public bool TryGetValue(string key, out string value)
        {
            return Values.TryGetValue(key, out value);
        }
    }
}
