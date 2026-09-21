using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class ConfigurationValidator
    {
        private readonly ProviderPlatformRules _platformRules = new ProviderPlatformRules();

        public IReadOnlyList<string> Validate(SdkConfiguration configuration)
        {
            List<string> issues = new List<string>();

            if (configuration == null)
            {
                issues.Add("No active configuration.");
                return issues;
            }

            if (configuration.Platform == PlatformId.YouTubePlayables && configuration.PlayerSettings.ApplyCompression && configuration.PlayerSettings.Compression != WebCompression.Disabled)
            {
                issues.Add("Compression Format must be Disabled for YouTube Playables.");
            }

            if (configuration.Platform == PlatformId.YouTubePlayables && configuration.PauseOnFocusLoss)
            {
                issues.Add("Pause on focus loss must be off for YouTube Playables: the game may resume only on the platform resume event.");
            }

            if (configuration.PlatformProvider == null)
            {
                issues.Add("The configuration has no platform provider.");
            }

            if (configuration.Languages.Count == 0)
            {
                issues.Add("The configuration has no languages.");
            }

            foreach (IProvider provider in Providers(configuration))
            {
                if (provider != null && _platformRules.IsAvailable(provider.GetType(), configuration.Platform) == false)
                {
                    issues.Add(provider.GetType().Name + " is not available on " + configuration.Platform + ".");
                }
            }

            return issues;
        }

        private IEnumerable<IProvider> Providers(SdkConfiguration configuration)
        {
            yield return configuration.PlatformProvider;
            yield return configuration.Ads;
            yield return configuration.Data;
            yield return configuration.Payments;
            yield return configuration.LanguageProvider;
            yield return configuration.Player;
            yield return configuration.Leaderboards;
            yield return configuration.Flags;
            yield return configuration.TimeProvider;
            yield return configuration.GameEvents;
            yield return configuration.Review;
            yield return configuration.GameLabel;
        }
    }
}
