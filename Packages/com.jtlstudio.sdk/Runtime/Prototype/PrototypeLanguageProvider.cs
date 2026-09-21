#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeLanguageProvider : ILanguageProvider
    {
        private readonly LanguageCodes _codes = new LanguageCodes();
        private readonly PrototypeSimulationSettings _settings;

        public PrototypeLanguageProvider(PrototypeSimulationSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string LanguageCode => _codes.ToCode(_settings.StartLanguage);

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }
    }
}
#endif
