using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YouTubePlayables)]
    public class YouTubePlayablesAdsProvider : BridgeProviderBase, IAdsProvider
    {
        public bool SupportsInterstitial => true;
        public bool SupportsRewarded => true;
        public bool SupportsBanner => false;
        public bool IsBannerVisible => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(IsBridgeReady ? ProviderState.Ready : ProviderState.Failed);
        }

        public void ShowInterstitial(Action<AdResult> onResult)
        {
            Call("ads", "showInterstitial", null, response => onResult(response.IsSuccess ? AdResult.Shown : AdResult.Failed));
        }

        public void ShowRewarded(string rewardId, Action<AdResult> onResult)
        {
            Call("ads", "showRewarded", new BridgePayload().Set("rewardId", rewardId), response =>
            {
                if (response.IsSuccess == false)
                {
                    onResult(AdResult.Failed);
                    return;
                }

                onResult(response.GetString("result") == "rewarded" ? AdResult.Rewarded : AdResult.Closed);
            });
        }

        public void ShowBanner()
        {
        }

        public void HideBanner()
        {
        }
    }
}
