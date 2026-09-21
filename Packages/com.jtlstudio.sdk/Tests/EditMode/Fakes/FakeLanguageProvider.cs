using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests.Fakes
{
    public class FakeLanguageProvider : ILanguageProvider
    {
        public FakeLanguageProvider(string languageCode)
        {
            LanguageCode = languageCode;
        }

        public string LanguageCode { get; }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }
    }
}
