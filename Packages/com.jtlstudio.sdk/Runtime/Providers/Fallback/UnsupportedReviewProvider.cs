using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class UnsupportedReviewProvider : IReviewProvider
    {
        public bool CanRequest => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Unsupported);
        }

        public void Request(Action<bool> onResult)
        {
            onResult(false);
        }
    }
}
