using System.Collections.Generic;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class ConfigurationValidator
    {
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

            if (configuration.PlatformProvider == null)
            {
                issues.Add("The configuration has no platform provider.");
            }

            if (configuration.Languages.Count == 0)
            {
                issues.Add("The configuration has no languages.");
            }

            return issues;
        }
    }
}
