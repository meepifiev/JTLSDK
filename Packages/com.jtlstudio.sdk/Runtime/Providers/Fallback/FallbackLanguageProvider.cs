using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class FallbackLanguageProvider : ILanguageProvider
    {
        public string LanguageCode => "";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }
    }
}
