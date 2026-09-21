using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class PlatformService : ModuleBase, IPlatform
    {
        private readonly IPlatformProvider _provider;
        private readonly AdsService _ads;
        private readonly PaymentsService _payments;
        private readonly LeaderboardsService _leaderboards;
        private readonly PlayerService _player;
        private readonly FlagsService _flags;
        private readonly TimeService _time;
        private readonly ReviewService _review;
        private readonly ShortcutService _shortcut;

        public PlatformService(
            IPlatformProvider provider,
            AdsService ads,
            PaymentsService payments,
            LeaderboardsService leaderboards,
            PlayerService player,
            FlagsService flags,
            TimeService time,
            ReviewService review,
            ShortcutService shortcut,
            SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _ads = ads ?? throw new ArgumentNullException(nameof(ads));
            _payments = payments ?? throw new ArgumentNullException(nameof(payments));
            _leaderboards = leaderboards ?? throw new ArgumentNullException(nameof(leaderboards));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _flags = flags ?? throw new ArgumentNullException(nameof(flags));
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _review = review ?? throw new ArgumentNullException(nameof(review));
            _shortcut = shortcut ?? throw new ArgumentNullException(nameof(shortcut));
        }

        public PlatformId Current => _provider.Platform;
        public string AppId => _provider.AppId ?? "";

        internal override string ModuleName => "Platform";

        public bool Supports(Capability capability)
        {
            switch (capability)
            {
                case Capability.Interstitial:
                    return _ads.IsInterstitialSupported;

                case Capability.Rewarded:
                    return _ads.IsRewardedSupported;

                case Capability.Banner:
                    return _ads.IsBannerSupported;

                case Capability.Purchases:
                    return _payments.IsSupported;

                case Capability.Leaderboards:
                    return _leaderboards.IsSupported;

                case Capability.LeaderboardsLoad:
                    return _leaderboards.CanLoad;

                case Capability.Authorization:
                    return _player.IsSupported;

                case Capability.Flags:
                    return _flags.IsSupported;

                case Capability.ServerTime:
                    return _time.IsServerTime;

                case Capability.Review:
                    return _review.IsSupported;

                case Capability.Shortcut:
                    return _shortcut.IsSupported;

                case Capability.PlatformMute:
                    return _provider.SupportsPlatformMute;

                default:
                    throw new ArgumentOutOfRangeException(nameof(capability));
            }
        }

        internal override void Initialize()
        {
            _provider.Initialize(CompleteInitialization);
        }
    }
}
