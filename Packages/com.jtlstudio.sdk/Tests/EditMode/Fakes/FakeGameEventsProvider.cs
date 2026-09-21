using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests.Fakes
{
    public class FakeGameEventsProvider : IGameEventsProvider
    {
        private Action<ProviderState> _onInitialized;

        public List<string> Calls { get; } = new List<string>();
        public bool CompleteImmediately { get; set; } = true;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            if (CompleteImmediately)
            {
                onInitialized(ProviderState.Ready);
                return;
            }

            _onInitialized = onInitialized;
        }

        public void Complete(ProviderState state)
        {
            Action<ProviderState> callback = _onInitialized;
            _onInitialized = null;
            callback?.Invoke(state);
        }

        public void ReportGameReady()
        {
            Calls.Add("ready");
        }

        public void ReportGameplayStart()
        {
            Calls.Add("start");
        }

        public void ReportGameplayStop()
        {
            Calls.Add("stop");
        }
    }
}
