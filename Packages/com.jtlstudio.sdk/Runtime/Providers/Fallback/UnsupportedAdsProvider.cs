using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class UnsupportedAdsProvider : IAdsProvider
    {
        public bool SupportsInterstitial => false;
        public bool SupportsRewarded => false;
        public bool SupportsBanner => false;
        public bool IsBannerVisible => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Unsupported);
        }

        public void ShowInterstitial(Action<AdResult> onResult)
        {
            onResult(AdResult.NotSupported);
        }

        public void ShowRewarded(string rewardId, Action<AdResult> onResult)
        {
            onResult(AdResult.NotSupported);
        }

        public void ShowBanner()
        {
        }

        public void HideBanner()
        {
        }
    }
}
