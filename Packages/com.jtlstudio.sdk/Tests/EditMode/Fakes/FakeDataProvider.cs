using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests
{
    public class FakeDataProvider : IDataProvider
    {
        public int MaxBytes { get; set; } = 200 * 1024;
        public int RecommendedBytes { get; set; } = 100 * 1024;
        public string Stored { get; set; } = "";
        public DataLoadResult LoadResult { get; set; } = DataLoadResult.Empty;
        public bool SaveSucceeds { get; set; } = true;
        public int SaveCalls { get; private set; }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void Load(Action<DataLoadResult, string> onLoaded)
        {
            onLoaded(LoadResult, Stored);
        }

        public void Save(string serialized, Action<bool> onSaved)
        {
            SaveCalls++;

            if (SaveSucceeds)
            {
                Stored = serialized;
            }

            onSaved(SaveSucceeds);
        }
    }
}
