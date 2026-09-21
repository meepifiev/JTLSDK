using System.IO;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class SettingsAssetService
    {
        public const string SettingsAssetPath = "Assets/Resources/JTLSDK/JTLSDKSettings.asset";
        public const string ConfigurationsFolder = "Assets/Settings/JTLSDK/Configurations";

        private readonly ProviderDefaults _defaults = new ProviderDefaults();
        private readonly DefineSymbolService _defineSymbols = new DefineSymbolService();
        private readonly PresetDefaults _presetDefaults = new PresetDefaults();
        private readonly PlayerSettingsPresetService _presets = new PlayerSettingsPresetService();

        public JTLSDKSettings Find()
        {
            return AssetDatabase.LoadAssetAtPath<JTLSDKSettings>(SettingsAssetPath);
        }

        public JTLSDKSettings GetOrCreate()
        {
            JTLSDKSettings settings = Find();

            if (settings != null)
            {
                return settings;
            }

            EnsureFolder(Path.GetDirectoryName(SettingsAssetPath));
            settings = ScriptableObject.CreateInstance<JTLSDKSettings>();
            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        public SdkConfiguration CreateConfiguration(PlatformId platform, string displayName)
        {
            EnsureFolder(ConfigurationsFolder);
            SdkConfiguration configuration = ScriptableObject.CreateInstance<SdkConfiguration>();
            configuration.DisplayName = displayName;
            configuration.Platform = platform;
            _defaults.Apply(configuration, platform);
            _presetDefaults.Apply(configuration.PlayerSettings, platform);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(ConfigurationsFolder + "/" + SanitizeFileName(displayName) + ".asset");
            AssetDatabase.CreateAsset(configuration, assetPath);
            AssetDatabase.SaveAssets();
            return configuration;
        }

        public void Activate(JTLSDKSettings settings, SdkConfiguration configuration)
        {
            settings.ActiveConfiguration = configuration;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            _defineSymbols.Apply(configuration == null ? "" : configuration.DefineSymbol);

            if (configuration != null)
            {
                _presets.Apply(configuration.PlayerSettings);
            }
        }

        private void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        private string SanitizeFileName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string result = string.IsNullOrWhiteSpace(name) ? "Configuration" : name.Trim();

            foreach (char character in invalid)
            {
                result = result.Replace(character, '_');
            }

            return result.Replace(' ', '_');
        }
    }
}
