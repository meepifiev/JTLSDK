using System.Collections.Generic;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.Tests.Fakes
{
    public class TestSettingsBuilder
    {
        private readonly List<Object> _created = new List<Object>();

        public FakePlatformProvider Platform { get; } = new FakePlatformProvider();
        public IAdsProvider Ads { get; set; }
        public IDataProvider Data { get; set; }
        public IPaymentsProvider Payments { get; set; }
        public IPlayerProvider Player { get; set; }
        public ILanguageProvider LanguageProvider { get; set; }
        public ILeaderboardsProvider Leaderboards { get; set; }
        public IFlagsProvider Flags { get; set; }
        public IGameEventsProvider GameEvents { get; set; }
        public Language DefaultLanguage { get; set; } = Language.English;
        public List<LanguageReplacement> Replacements { get; } = new List<LanguageReplacement>();
        public List<LeaderboardDefinition> LeaderboardDefinitions { get; } = new List<LeaderboardDefinition>();
        public List<FlagDefinition> FlagDefinitions { get; } = new List<FlagDefinition>();
        public float InitializationTimeoutSeconds { get; set; } = 15f;
        public float AutosaveDelaySeconds { get; set; } = 2f;
        public List<ProductDefinition> Products { get; } = new List<ProductDefinition>();
        public List<Language> SupportedLanguages { get; } = new List<Language> { Language.English, Language.Russian };

        public JTLSDKSettings Build()
        {
            JTLSDKSettings settings = ScriptableObject.CreateInstance<JTLSDKSettings>();
            SdkConfiguration configuration = ScriptableObject.CreateInstance<SdkConfiguration>();
            _created.Add(settings);
            _created.Add(configuration);

            configuration.DisplayName = "Test";
            configuration.Platform = Platform.Platform;
            configuration.PlatformProvider = Platform;
            configuration.Ads = Ads;
            configuration.Data = Data;
            configuration.Payments = Payments;
            configuration.Player = Player;
            configuration.LanguageProvider = LanguageProvider;
            configuration.Leaderboards = Leaderboards;
            configuration.Flags = Flags;
            configuration.GameEvents = GameEvents;
            configuration.Languages.Clear();
            configuration.Languages.AddRange(SupportedLanguages);

            settings.ActiveConfiguration = configuration;
            settings.InitializationTimeoutSeconds = InitializationTimeoutSeconds;
            settings.AutosaveDelaySeconds = AutosaveDelaySeconds;
            settings.LogLevel = LogLevel.None;
            settings.UsePrototypesInEditor = false;
            settings.SupportedLanguages.Clear();
            settings.SupportedLanguages.AddRange(SupportedLanguages);
            settings.Products.AddRange(Products);
            settings.DefaultLanguage = DefaultLanguage;
            settings.LanguageReplacements.AddRange(Replacements);
            settings.Leaderboards.AddRange(LeaderboardDefinitions);
            settings.Flags.AddRange(FlagDefinitions);

            return settings;
        }

        public void Cleanup()
        {
            JTLSDK.Destroy();

            foreach (Object created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }
    }
}
