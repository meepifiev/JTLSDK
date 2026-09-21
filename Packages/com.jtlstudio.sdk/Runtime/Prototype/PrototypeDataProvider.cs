#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeDataProvider : IDataProvider
    {
        public const string StorageKey = "JTLSDK.Data";

        private readonly PrototypeSimulationSettings _settings;

        public PrototypeDataProvider(int maxBytes, int recommendedBytes, PrototypeSimulationSettings settings)
        {
            MaxBytes = maxBytes;
            RecommendedBytes = recommendedBytes;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public int MaxBytes { get; }
        public int RecommendedBytes { get; }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void Load(Action<DataLoadResult, string> onLoaded)
        {
            if (_settings.SimulateLoadFailure)
            {
                onLoaded(DataLoadResult.Failed, "");
                return;
            }

            if (_settings.EmptySaveOnStart)
            {
                onLoaded(DataLoadResult.Empty, "");
                return;
            }

            string stored = PlayerPrefs.GetString(StorageKey, "");
            onLoaded(string.IsNullOrEmpty(stored) ? DataLoadResult.Empty : DataLoadResult.Loaded, stored);
        }

        public void Save(string serialized, Action<bool> onSaved)
        {
            PlayerPrefs.SetString(StorageKey, serialized);
            PlayerPrefs.Save();
            onSaved(true);
        }
    }
}
#endif
