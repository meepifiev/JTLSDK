using System;

namespace JTLStudio.SDK
{
    public interface IAds : IModule
    {
        bool IsShowing { get; }
        bool IsInterstitialSupported { get; }
        bool IsRewardedSupported { get; }
        bool IsBannerSupported { get; }
        bool IsBannerVisible { get; }

        event Action Opened;
        event Action Closed;

        void ShowInterstitial(Action<AdResult> onResult = null);
        void ShowRewarded(string rewardId, Action<AdResult> onResult);
        void ShowBanner();
        void HideBanner();
    }
}
