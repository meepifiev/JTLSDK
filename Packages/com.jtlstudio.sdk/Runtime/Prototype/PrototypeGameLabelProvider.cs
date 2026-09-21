#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeGameLabelProvider : IGameLabelProvider
    {
        private bool _requested;

        public bool CanShow => _requested == false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void ShowDialog(Action<bool> onResult)
        {
            _requested = true;
            onResult(true);
        }
    }
}
#endif
