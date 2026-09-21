using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class UnsupportedDataProvider : IDataProvider
    {
        public int MaxBytes => 0;
        public int RecommendedBytes => 0;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Unsupported);
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
