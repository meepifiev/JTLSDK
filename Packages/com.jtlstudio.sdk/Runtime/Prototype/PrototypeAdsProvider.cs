#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeAdsProvider : IAdsProvider
    {
        private readonly PrototypeSimulationSettings _settings;
        private readonly PlatformId _platform;

        public PrototypeAdsProvider(bool supportsInterstitial, bool supportsRewarded, bool supportsBanner, PlatformId platform, PrototypeSimulationSettings settings)
        {
            SupportsInterstitial = supportsInterstitial;
            SupportsRewarded = supportsRewarded;
            SupportsBanner = supportsBanner;
            _platform = platform;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool SupportsInterstitial { get; }
        public bool SupportsRewarded { get; }
        public bool SupportsBanner { get; }
        public bool IsBannerVisible { get; private set; }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void ShowInterstitial(Action<AdResult> onResult)
        {
            Show(false, "", _settings.InterstitialResult, onResult);
        }

        public void ShowRewarded(string rewardId, Action<AdResult> onResult)
        {
            Show(true, rewardId, _settings.RewardedResult, onResult);
        }

        public void ShowBanner()
        {
            IsBannerVisible = true;
        }

        public void HideBanner()
        {
            IsBannerVisible = false;
        }

        private void Show(bool rewarded, string rewardId, AdResult selectedResult, Action<AdResult> onResult)
        {
            if (_settings.AskAdResult && PrototypeBridge.HasAdPresenter)
            {
                PrototypeBridge.RequestAd(new PrototypeAdRequest(rewarded, rewardId, _platform, onResult));
                return;
            }

            new PrototypeTimer(_settings.AdDurationSeconds, () => onResult(selectedResult));
        }
    }
}
#endif
