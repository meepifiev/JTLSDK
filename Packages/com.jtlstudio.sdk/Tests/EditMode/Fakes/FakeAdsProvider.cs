using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests.Fakes
{
    public class FakeAdsProvider : IAdsProvider
    {
        private Action<ProviderState> _onInitialized;
        private Action<AdResult> _pendingResult;

        public bool SupportsInterstitial { get; set; } = true;
        public bool SupportsRewarded { get; set; } = true;
        public bool SupportsBanner { get; set; }
        public bool IsBannerVisible { get; private set; }
        public bool CompleteImmediately { get; set; } = true;
        public int ShowCount { get; private set; }
        public string LastRewardId { get; private set; }
        public bool HasPendingShow => _pendingResult != null;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            if (CompleteImmediately)
            {
                onInitialized(ProviderState.Ready);
                return;
            }

            _onInitialized = onInitialized;
        }

        public void Complete(ProviderState state)
        {
            Action<ProviderState> callback = _onInitialized;
            _onInitialized = null;
            callback?.Invoke(state);
        }

        public void ShowInterstitial(Action<AdResult> onResult)
        {
            ShowCount++;
            _pendingResult = onResult;
        }

        public void ShowRewarded(string rewardId, Action<AdResult> onResult)
        {
            ShowCount++;
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
