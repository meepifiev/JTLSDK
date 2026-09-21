#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeFactory
    {
        private readonly PlatformId _platform;
        private readonly PrototypeSimulationSettings _simulation = new PrototypeSimulationSettings();

        public PrototypeFactory(PlatformId platform, JTLSDKSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _platform = platform;
            _simulation.Load();
        }

        public IPlatformProvider Platform(IPlatformProvider configured)
        {
            return new PrototypePlatformProvider(_platform, configured.SupportsPlatformMute, _simulation);
        }

        public IAdsProvider Ads(IAdsProvider configured)
        {
            if (configured is UnsupportedAdsProvider)
            {
                return configured;
            }

            return new PrototypeAdsProvider(configured.SupportsInterstitial, configured.SupportsRewarded, configured.SupportsBanner, _platform, _simulation);
        }

        public IDataProvider Data(IDataProvider configured)
        {
            if (configured is UnsupportedDataProvider)
            {
                return new PrototypeDataProvider(0, 0, _simulation);
            }

            return new PrototypeDataProvider(configured.MaxBytes, configured.RecommendedBytes, _simulation);
        }

        public IPaymentsProvider Payments(IPaymentsProvider configured)
        {
            if (configured is UnsupportedPaymentsProvider)
            {
                return configured;
            }

            return new PrototypePaymentsProvider(_platform, _simulation);
        }

        public ILanguageProvider Language(ILanguageProvider configured)
        {
            return new PrototypeLanguageProvider(_simulation);
        }

        public IPlayerProvider Player(IPlayerProvider configured)
        {
            if (configured is UnsupportedPlayerProvider)
            {
                return configured;
            }

            return new PrototypePlayerProvider(_simulation);
        }

        public ILeaderboardsProvider Leaderboards(ILeaderboardsProvider configured)
        {
            if (configured is UnsupportedLeaderboardsProvider)
            {
                return configured;
            }

            return new PrototypeLeaderboardsProvider(configured.SupportsLoad, _simulation);
        }

        public IFlagsProvider Flags(IFlagsProvider configured)
        {
            if (configured is UnsupportedFlagsProvider)
            {
                return configured;
            }

            return new PrototypeFlagsProvider();
        }

        public ITimeProvider Time(ITimeProvider configured)
        {
            return new PrototypeTimeProvider(configured.IsServerTime);
        }

        public IGameEventsProvider GameEvents(IGameEventsProvider configured)
        {
            return new PrototypeGameEventsProvider();
        }

        public IReviewProvider Review(IReviewProvider configured)
        {
            if (configured is UnsupportedReviewProvider)
            {
                return configured;
            }

            return new PrototypeReviewProvider();
        }

        public IGameLabelProvider GameLabel(IGameLabelProvider configured)
        {
            if (configured is UnsupportedGameLabelProvider)
            {
                return configured;
            }

            return new PrototypeGameLabelProvider();
        }
    }
}
#endif
