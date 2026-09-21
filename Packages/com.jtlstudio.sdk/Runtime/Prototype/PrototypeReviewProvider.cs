#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeReviewProvider : IReviewProvider
    {
        private bool _requested;

        public bool CanRequest => _requested == false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void Request(Action<bool> onResult)
        {
            _requested = true;
            onResult(true);
        }
    }
}
#endif
