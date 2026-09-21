using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YouTubePlayables)]
    public class YouTubePlayablesDataProvider : BridgeProviderBase, IDataProvider
    {
        private const int PlatformLimitBytes = 3 * 1024 * 1024;
        private const int RecommendedLimitBytes = 500 * 1024;

        public int MaxBytes => PlatformLimitBytes;
        public int RecommendedBytes => RecommendedLimitBytes;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(IsBridgeReady ? ProviderState.Ready : ProviderState.Failed);
        }

        public void Load(Action<DataLoadResult, string> onLoaded)
        {
            Call("data", "load", null, response =>
            {
                if (response.IsSuccess == false)
                {
                    onLoaded(DataLoadResult.Failed, "");
                    return;
                }

                bool loaded = response.GetString("result") == "loaded";
                onLoaded(loaded ? DataLoadResult.Loaded : DataLoadResult.Empty, loaded ? response.GetString("data") : "");
            });
        }

        public void Save(string serialized, Action<bool> onSaved)
        {
            Call("data", "save", new BridgePayload().Set("data", serialized), response => onSaved(response.IsSuccess));
        }
    }
}
