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
