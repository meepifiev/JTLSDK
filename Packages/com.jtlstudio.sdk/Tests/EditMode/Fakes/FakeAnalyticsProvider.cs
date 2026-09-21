using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests.Fakes
{
    public class FakeAnalyticsProvider : IAnalyticsProvider
    {
        public ProviderState InitialState { get; set; } = ProviderState.Ready;
        public List<string> Events { get; } = new List<string>();

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(InitialState);
        }

        public void Report(string eventName, string parametersJson)
        {
            Events.Add(string.IsNullOrEmpty(parametersJson) ? eventName : eventName + " " + parametersJson);
        }
    }
}
