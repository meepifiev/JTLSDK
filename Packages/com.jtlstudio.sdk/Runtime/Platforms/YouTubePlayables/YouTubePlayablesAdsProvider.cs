using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    public class YouTubePlayablesAdsProvider : IAdsProvider
    {
        public bool SupportsInterstitial => true;
        public bool SupportsRewarded => true;
        public bool SupportsBanner => false;
        public bool IsBannerVisible => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }

        public void ShowInterstitial(Action<AdResult> onResult)
        {
            onResult(AdResult.Failed);
        }

        public void ShowRewarded(string rewardId, Action<AdResult> onResult)
        {
            onResult(AdResult.Failed);
        }

        public void ShowBanner()
        {
        }

        public void HideBanner()
        {
        }
    }
}
