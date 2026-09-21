using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class AdsService : ModuleBase, IAds
    {
        private readonly IAdsProvider _provider;
        private readonly PauseService _pause;
        private readonly GameEventsService _gameEvents;

        public AdsService(IAdsProvider provider, PauseService pause, GameEventsService gameEvents, SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
            _gameEvents = gameEvents ?? throw new ArgumentNullException(nameof(gameEvents));
        }

        public event Action Opened;
        public event Action Closed;

        public bool IsShowing { get; private set; }
        public bool IsInterstitialSupported => IsSupported && _provider.SupportsInterstitial;
        public bool IsRewardedSupported => IsSupported && _provider.SupportsRewarded;
        public bool IsBannerSupported => IsSupported && _provider.SupportsBanner;
        public bool IsBannerVisible => IsBannerSupported && _provider.IsBannerVisible;

        internal override string ModuleName => "Ads";

        public void ShowInterstitial(Action<AdResult> onResult = null)
        {
            if (TryReject(_provider.SupportsInterstitial, out AdResult rejection))
            {
                onResult?.Invoke(rejection);
                return;
            }

            AdShow show = BeginShow(onResult);

            try
            {
                _provider.ShowInterstitial(show.Complete);
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
                show.Complete(AdResult.Failed);
            }
        }

        public void ShowRewarded(string rewardId, Action<AdResult> onResult)
        {
            if (string.IsNullOrEmpty(rewardId))
            {
                throw new ArgumentException(nameof(rewardId));
            }

            if (onResult == null)
            {
                throw new ArgumentNullException(nameof(onResult));
            }

            if (TryReject(_provider.SupportsRewarded, out AdResult rejection))
            {
                onResult(rejection);
                return;
            }

            AdShow show = BeginShow(onResult);

            try
            {
                _provider.ShowRewarded(rewardId, show.Complete);
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
                show.Complete(AdResult.Failed);
            }
        }

        public void ShowBanner()
        {
            if (RejectIfNotReady(nameof(ShowBanner)) || IsBannerSupported == false)
            {
                return;
            }

            _provider.ShowBanner();
        }

        public void HideBanner()
        {
            if (RejectIfNotReady(nameof(HideBanner)) || IsBannerSupported == false)
            {
                return;
            }

            _provider.HideBanner();
        }

        internal override void Initialize()
        {
            _provider.Initialize(CompleteInitialization);
        }

        internal void EndShow(IDisposable pauseHold, Action<AdResult> onResult, AdResult result)
        {
            IsShowing = false;
            _gameEvents.Resume();
            pauseHold.Dispose();
            Invoke(Closed);

            try
            {
                onResult?.Invoke(result);
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
            }
        }

        private bool TryReject(bool formatSupported, out AdResult rejection)
        {
            if (IsReady == false)
            {
                rejection = AdResult.NotReady;
                return true;
            }

            if (IsSupported == false || formatSupported == false)
            {
                rejection = AdResult.NotSupported;
                return true;
            }

            if (IsShowing)
            {
                Logger.Warning("An ad is already showing.");
                rejection = AdResult.NotShown;
                return true;
            }

            rejection = AdResult.Failed;
            return false;
        }

        private AdShow BeginShow(Action<AdResult> onResult)
        {
            IsShowing = true;
            IDisposable pauseHold = _pause.Hold(PauseSources.Advertisement);
            _gameEvents.Suspend();
            Invoke(Opened);
            return new AdShow(this, pauseHold, onResult);
        }

        private void Invoke(Action handlers)
        {
            try
            {
                handlers?.Invoke();
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
            }
        }
    }
}
