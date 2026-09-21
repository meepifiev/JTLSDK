using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YouTubePlayables)]
    public class YouTubePlayablesLanguageProvider : BridgeProviderBase, ILanguageProvider
    {
        public string LanguageCode { get; private set; } = "";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            InitializeWith("platform", "initialize", onInitialized, response => LanguageCode = response.GetString("language"));
        }
    }
}
