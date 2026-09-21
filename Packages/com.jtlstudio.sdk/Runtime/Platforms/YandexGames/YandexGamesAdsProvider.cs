using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesAdsProvider : BridgeProviderBase, IAdsProvider
    {
        [SerializeField] private bool _adBlockDetection = true;
        [SerializeField] private int _minimumInterstitialIntervalSeconds = 60;
        [SerializeField] private int _skipInterstitialAfterRewardedSeconds = 60;
        [SerializeField] private bool _stickyBanner;

        private DateTime _lastInterstitial = DateTime.MinValue;
        private DateTime _lastRewarded = DateTime.MinValue;

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
            if (IsBridgeReady == false)
            {
                onInitialized(ProviderState.Failed);
                return;
            }

            if (_stickyBanner)
            {
                ShowBanner();
            }

            onInitialized(ProviderState.Ready);
        }

        public void ShowInterstitial(Action<AdResult> onResult)
        {
            if (IsInterstitialOnCooldown())
            {
                onResult(AdResult.NotShown);
                return;
            }

            Call("ads", "showInterstitial", new BridgePayload().Set("adBlockDetection", _adBlockDetection), response =>
            {
                AdResult result = MapInterstitial(response);

                if (result == AdResult.Shown)
                {
                    _lastInterstitial = DateTime.UtcNow;
                }

                onResult(result);
            });
        }

        public void ShowRewarded(string rewardId, Action<AdResult> onResult)
        {
            Call("ads", "showRewarded", new BridgePayload().Set("rewardId", rewardId).Set("adBlockDetection", _adBlockDetection), response =>
            {
                AdResult result = MapRewarded(response);

                if (result == AdResult.Rewarded)
                {
                    _lastRewarded = DateTime.UtcNow;
                }

                onResult(result);
            });
        }

        public void ShowBanner()
        {
            Call("ads", "showBanner", null, response => IsBannerVisible = response.IsSuccess);
        }

        public void HideBanner()
        {
            Call("ads", "hideBanner", null, _ => IsBannerVisible = false);
        }

        private bool IsInterstitialOnCooldown()
        {
            DateTime now = DateTime.UtcNow;
            bool intervalActive = (now - _lastInterstitial).TotalSeconds < _minimumInterstitialIntervalSeconds;
            bool rewardedRecently = (now - _lastRewarded).TotalSeconds < _skipInterstitialAfterRewardedSeconds;
            return intervalActive || rewardedRecently;
        }

        private AdResult MapInterstitial(BridgeResponse response)
        {
            if (response.IsSuccess == false)
            {
                return AdResult.Failed;
            }

            switch (response.GetString("result"))
            {
                case "shown":
                    return AdResult.Shown;

                case "blocked":
                    return AdResult.Blocked;

                default:
                    return AdResult.NotShown;
            }
        }

        private AdResult MapRewarded(BridgeResponse response)
        {
            if (response.IsSuccess == false)
            {
                return AdResult.Failed;
            }

            switch (response.GetString("result"))
            {
                case "rewarded":
                    return AdResult.Rewarded;

                case "closed":
                    return AdResult.Closed;

                case "blocked":
                    return AdResult.Blocked;

                default:
                    return AdResult.NotShown;
            }
        }
    }
}
