using JTLStudio.SDK.Providers;
using JTLStudio.SDK.YandexGames;
using JTLStudio.SDK.YouTubePlayables;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class ProviderDefaults
    {
        public void Apply(SdkConfiguration configuration, PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.YandexGames:
                    configuration.PlatformProvider = new YandexGamesPlatformProvider();
                    configuration.Ads = new YandexGamesAdsProvider();
                    configuration.Data = new YandexGamesDataProvider();
                    configuration.Payments = new YandexGamesPaymentsProvider();
                    configuration.LanguageProvider = new YandexGamesLanguageProvider();
                    configuration.Player = new YandexGamesPlayerProvider();
                    configuration.Leaderboards = new YandexGamesLeaderboardsProvider();
                    configuration.Flags = new YandexGamesFlagsProvider();
                    configuration.TimeProvider = new YandexGamesTimeProvider();
                    configuration.Gameplay = new YandexGamesGameplayProvider();
                    configuration.Review = new YandexGamesReviewProvider();
                    configuration.Shortcut = new YandexGamesShortcutProvider();
                    configuration.DefineSymbol = "JTLSDK_YANDEX_GAMES";
                    break;

                case PlatformId.YouTubePlayables:
                    configuration.PlatformProvider = new YouTubePlayablesPlatformProvider();
                    configuration.Ads = new YouTubePlayablesAdsProvider();
                    configuration.Data = new YouTubePlayablesDataProvider();
                    configuration.Payments = new UnsupportedPaymentsProvider();
                    configuration.LanguageProvider = new YouTubePlayablesLanguageProvider();
                    configuration.Player = new UnsupportedPlayerProvider();
                    configuration.Leaderboards = new YouTubePlayablesLeaderboardsProvider();
                    configuration.Flags = new UnsupportedFlagsProvider();
                    configuration.TimeProvider = new FallbackTimeProvider();
                    configuration.Gameplay = new YouTubePlayablesGameplayProvider();
                    configuration.Review = new UnsupportedReviewProvider();
                    configuration.Shortcut = new UnsupportedShortcutProvider();
                    configuration.DefineSymbol = "JTLSDK_YOUTUBE_PLAYABLES";
                    break;

                default:
                    configuration.PlatformProvider = new FallbackPlatformProvider();
                    configuration.Ads = new UnsupportedAdsProvider();
                    configuration.Data = new UnsupportedDataProvider();
                    configuration.Payments = new UnsupportedPaymentsProvider();
                    configuration.LanguageProvider = new FallbackLanguageProvider();
                    configuration.Player = new UnsupportedPlayerProvider();
                    configuration.Leaderboards = new UnsupportedLeaderboardsProvider();
                    configuration.Flags = new UnsupportedFlagsProvider();
                    configuration.TimeProvider = new FallbackTimeProvider();
                    configuration.Gameplay = new FallbackGameplayProvider();
                    configuration.Review = new UnsupportedReviewProvider();
                    configuration.Shortcut = new UnsupportedShortcutProvider();
                    configuration.DefineSymbol = "";
                    break;
            }
        }
    }
}
