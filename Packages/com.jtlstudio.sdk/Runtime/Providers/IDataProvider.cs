using System;

namespace JTLStudio.SDK.Providers
{
    public interface IDataProvider : IProvider
    {
        int MaxBytes { get; }
        int RecommendedBytes { get; }

        void Load(Action<DataLoadResult, string> onLoaded);
        void Save(string serialized, Action<bool> onSaved);
    }
}
