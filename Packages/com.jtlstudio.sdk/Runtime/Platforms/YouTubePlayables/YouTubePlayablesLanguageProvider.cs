using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    public class YouTubePlayablesLanguageProvider : ILanguageProvider
    {
        public string LanguageCode => "";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }
    }
}
