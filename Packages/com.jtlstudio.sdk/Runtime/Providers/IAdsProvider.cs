using System;

namespace JTLStudio.SDK.Providers
{
    public interface IAdsProvider : IProvider
    {
        bool SupportsInterstitial { get; }
        bool SupportsRewarded { get; }
        bool SupportsBanner { get; }
        bool IsBannerVisible { get; }

        void ShowInterstitial(Action<AdResult> onResult);
        void ShowRewarded(string rewardId, Action<AdResult> onResult);
        void ShowBanner();
        void HideBanner();
    }
}
