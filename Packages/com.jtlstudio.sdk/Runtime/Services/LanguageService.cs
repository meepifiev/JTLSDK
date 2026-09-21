using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class LanguageService : ModuleBase, ILanguage
    {
        private readonly ILanguageProvider _provider;
        private readonly JTLSDKSettings _settings;
        private readonly LanguageCodes _codes = new LanguageCodes();
        private readonly List<Language> _supported = new List<Language>();

        public LanguageService(ILanguageProvider provider, JTLSDKSettings settings, SdkConfiguration configuration, SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            foreach (Language language in settings.SupportedLanguages)
            {
                bool allowedByConfiguration = configuration == null || configuration.Languages.Contains(language);

                if (allowedByConfiguration && _supported.Contains(language) == false)
                {
                    _supported.Add(language);
                }
            }

            if (_supported.Count == 0)
            {
                _supported.Add(settings.DefaultLanguage);
            }

            Current = _supported.Contains(settings.DefaultLanguage) ? settings.DefaultLanguage : _supported[0];
        }

        public event Action<Language> Changed;

        public Language Current { get; private set; }
        public IReadOnlyList<Language> Supported => _supported;

        internal override string ModuleName => "Language";

        public void Set(Language language)
        {
            if (_supported.Contains(language) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(language));
            }

            if (Current == language)
            {
                return;
            }

            Current = language;
            Changed?.Invoke(language);
        }

        internal override void Initialize()
        {
            _provider.Initialize(OnProviderInitialized);
        }

        private void OnProviderInitialized(ProviderState state)
        {
            if (state == ProviderState.Ready)
            {
                Current = Resolve(_provider.LanguageCode);
            }

            CompleteInitialization(state);
        }

        private Language Resolve(string code)
        {
            if (_codes.TryParse(code, out Language language) == false)
            {
                return Current;
            }

            if (_supported.Contains(language))
            {
                return language;
            }

            foreach (LanguageReplacement replacement in _settings.LanguageReplacements)
            {
                if (replacement.From == language && _supported.Contains(replacement.To))
                {
                    return replacement.To;
                }
            }

            return Current;
        }
    }
}
