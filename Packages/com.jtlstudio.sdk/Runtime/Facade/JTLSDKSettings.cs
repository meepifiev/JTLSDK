using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK
{
    public class JTLSDKSettings : ScriptableObject
    {
        public const string ResourcePath = "JTLSDK/JTLSDKSettings";

        [SerializeField] private SdkConfiguration _activeConfiguration;
        [SerializeField] private float _initializationTimeoutSeconds = 15f;
        [SerializeField] private float _autosaveDelaySeconds = 2f;
        [SerializeField] private LogLevel _logLevel = LogLevel.ErrorsAndWarnings;
        [SerializeField] private Language _defaultLanguage = Language.English;
        [SerializeField] private List<Language> _supportedLanguages = new List<Language> { Language.English };
        [SerializeField] private List<LanguageReplacement> _languageReplacements = new List<LanguageReplacement>();
        [SerializeField] private List<ProductDefinition> _products = new List<ProductDefinition>();
        [SerializeField] private List<LeaderboardDefinition> _leaderboards = new List<LeaderboardDefinition>();
        [SerializeField] private List<FlagDefinition> _flags = new List<FlagDefinition>();

        public SdkConfiguration ActiveConfiguration
        {
            get => _activeConfiguration;
            internal set => _activeConfiguration = value;
        }

        public float InitializationTimeoutSeconds
        {
            get => _initializationTimeoutSeconds;
            internal set => _initializationTimeoutSeconds = value;
        }

        public float AutosaveDelaySeconds
        {
            get => _autosaveDelaySeconds;
            internal set => _autosaveDelaySeconds = value;
        }

        public LogLevel LogLevel
        {
            get => _logLevel;
            internal set => _logLevel = value;
        }

        public Language DefaultLanguage
        {
            get => _defaultLanguage;
            internal set => _defaultLanguage = value;
        }

        public List<Language> SupportedLanguages => _supportedLanguages;
        public List<LanguageReplacement> LanguageReplacements => _languageReplacements;
        public List<ProductDefinition> Products => _products;
        public List<LeaderboardDefinition> Leaderboards => _leaderboards;
        public List<FlagDefinition> Flags => _flags;
    }
}
