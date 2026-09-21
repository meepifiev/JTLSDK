using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests
{
    public class FakeAdsProvider : IAdsProvider
    {
        private Action<AdResult> _pendingResult;

        public bool SupportsInterstitial { get; set; } = true;
        public bool SupportsRewarded { get; set; } = true;
        public bool SupportsBanner { get; set; }
        public bool IsBannerVisible { get; private set; }
        public int ShowCalls { get; private set; }
        public string LastRewardId { get; private set; }
        public bool HasPendingShow => _pendingResult != null;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void ShowInterstitial(Action<AdResult> onResult)
        {
            ShowCalls++;
            _pendingResult = onResult;
        }

        public void ShowRewarded(string rewardId, Action<AdResult> onResult)
        {
            ShowCalls++;
            LastRewardId = rewardId;
            _pendingResult = onResult;
        }

        public void ShowBanner()
        {
            IsBannerVisible = true;
        }

        public void HideBanner()
        {
            IsBannerVisible = false;
        }

        public void CompleteShow(AdResult result)
        {
            Action<AdResult> callback = _pendingResult;
            _pendingResult = null;
            callback?.Invoke(result);
        }
    }
}
