using JTLStudio.SDK.Editor.Configuration;
using UnityEditor;

namespace JTLStudio.SDK.Editor.Development
{
    public class DemoSettingsSetup
    {
        private readonly SettingsAssetService _assets = new SettingsAssetService();

        public JTLSDKSettings Create()
        {
            JTLSDKSettings settings = _assets.GetOrCreate();

            if (settings.ActiveConfiguration == null)
            {
                SdkConfiguration configuration = _assets.CreateConfiguration(PlatformId.YandexGames, "Yandex Games");
                configuration.Languages.Clear();
                configuration.Languages.Add(Language.English);
                configuration.Languages.Add(Language.Russian);
                EditorUtility.SetDirty(configuration);
                _assets.Activate(settings, configuration);
            }

            if (settings.Products.Count == 0)
            {
                settings.Products.Add(new ProductDefinition("remove_ads", ProductType.NonConsumable));
                settings.Products.Add(new ProductDefinition("coins_1000", ProductType.Consumable));
            }

            if (settings.Leaderboards.Count == 0)
            {
                settings.Leaderboards.Add(new LeaderboardDefinition("levels"));
            }

            if (settings.Flags.Count == 0)
            {
                settings.Flags.Add(new FlagDefinition("tutorial_enabled", FlagType.Bool, "true"));
            }

            if (settings.SupportedLanguages.Contains(Language.Russian) == false)
            {
                settings.SupportedLanguages.Add(Language.Russian);
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
