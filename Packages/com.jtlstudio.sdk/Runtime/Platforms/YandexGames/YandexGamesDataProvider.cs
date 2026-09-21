using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesDataProvider : IDataProvider
    {
        private const int PlatformLimitBytes = 200 * 1024;
        private const int RecommendedLimitBytes = 100 * 1024;

        public int MaxBytes => PlatformLimitBytes;
        public int RecommendedBytes => RecommendedLimitBytes;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }

        public void Load(Action<DataLoadResult, string> onLoaded)
        {
            onLoaded(DataLoadResult.Failed, "");
        }

        public void Save(string serialized, Action<bool> onSaved)
        {
            onSaved(false);
        }
    }
}
