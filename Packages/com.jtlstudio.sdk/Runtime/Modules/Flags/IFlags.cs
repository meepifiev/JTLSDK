namespace JTLStudio.SDK
{
    public interface IFlags : IModule
    {
        bool HasKey(string key);
        bool GetBool(string key, bool defaultValue = false);
        int GetInt(string key, int defaultValue = 0);
        float GetFloat(string key, float defaultValue = 0f);
        string GetString(string key, string defaultValue = "");
    }
}
