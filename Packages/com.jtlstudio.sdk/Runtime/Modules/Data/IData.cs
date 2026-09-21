using System;

namespace JTLStudio.SDK
{
    public interface IData : IModule
    {
        DataState LoadState { get; }
        bool IsDirty { get; }

        event Action Loaded;
        event Action<bool> Flushed;

        bool HasKey(string key);
        int GetInt(string key, int defaultValue = 0);
        float GetFloat(string key, float defaultValue = 0f);
        bool GetBool(string key, bool defaultValue = false);
        string GetString(string key, string defaultValue = "");
        T GetObject<T>(string key, T defaultValue = null) where T : class;

        void SetInt(string key, int value);
        void SetFloat(string key, float value);
        void SetBool(string key, bool value);
        void SetString(string key, string value);
        void SetObject<T>(string key, T value) where T : class;

        void DeleteKey(string key);
        void DeleteAll();

        void Save();
        void Flush(Action<bool> onFlushed = null);
    }
}
