using System;
using System.Collections.Generic;
using JTLStudio.SDK.Editor.Configuration;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Editor.Toolkit.Data
{
    public class ToolkitProject
    {
        private readonly SettingsAssetService _assets = new SettingsAssetService();
        private readonly PlatformPresentation _platforms = new PlatformPresentation();

        public event Action Changed;

        public JTLSDKSettings Settings => _assets.GetOrCreate();

        public SdkConfiguration Active => Settings.ActiveConfiguration;

        public IReadOnlyList<SdkConfiguration> Configurations
        {
            get
            {
                List<SdkConfiguration> configurations = new List<SdkConfiguration>();

                foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(SdkConfiguration)))
                {
                    SdkConfiguration configuration = AssetDatabase.LoadAssetAtPath<SdkConfiguration>(AssetDatabase.GUIDToAssetPath(guid));

                    if (configuration != null)
                    {
                        configurations.Add(configuration);
                    }
                }

                configurations.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
                return configurations;
            }
        }

        public SdkConfiguration Create(PlatformId platform)
        {
            SdkConfiguration configuration = _assets.CreateConfiguration(platform, _platforms.DisplayName(platform));
            configuration.Languages.Clear();
            configuration.Languages.AddRange(Settings.SupportedLanguages);
            Save(configuration);

            if (Active == null)
            {
                Activate(configuration);
            }

            NotifyChanged();
            return configuration;
        }

        public void Activate(SdkConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            Undo.RecordObject(Settings, "Activate JTL SDK configuration");
            _assets.Activate(Settings, configuration);
            NotifyChanged();
        }

        public void Delete(SdkConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (Active == configuration)
            {
                _assets.Activate(Settings, null);
            }

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(configuration));
            NotifyChanged();
        }

        public void Save(UnityEngine.Object target)
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        public void Modify(UnityEngine.Object target, string actionName, Action change)
        {
            Undo.RecordObject(target, actionName);
            change();
            Save(target);
            NotifyChanged();
        }

        public void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
