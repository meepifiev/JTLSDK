using System;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesAdsProvider : IAdsProvider
    {
        [SerializeField] private bool _adBlockDetection = true;
        [SerializeField] private int _minimumInterstitialIntervalSeconds = 60;
        [SerializeField] private int _skipInterstitialAfterRewardedSeconds = 60;
        [SerializeField] private bool _stickyBanner;

        public bool AdBlockDetection => _adBlockDetection;
        public int MinimumInterstitialIntervalSeconds => _minimumInterstitialIntervalSeconds;
        public int SkipInterstitialAfterRewardedSeconds => _skipInterstitialAfterRewardedSeconds;
        public bool StickyBanner => _stickyBanner;

        public bool SupportsInterstitial => true;
        public bool SupportsRewarded => true;
        public bool SupportsBanner => true;
        public bool IsBannerVisible { get; private set; }

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
            IsBannerVisible = true;
        }

        public void HideBanner()
        {
            IsBannerVisible = false;
        }
    }
}
