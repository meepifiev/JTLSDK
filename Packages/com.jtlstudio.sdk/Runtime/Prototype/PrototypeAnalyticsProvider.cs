#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeAnalyticsProvider : IAnalyticsProvider
    {
        private const string Prefix = "[JTL SDK] Analytics · ";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void Report(string eventName, string parametersJson)
        {
            Debug.Log(string.IsNullOrEmpty(parametersJson) ? Prefix + eventName : Prefix + eventName + " · " + parametersJson);
        }
    }
}
#endif
