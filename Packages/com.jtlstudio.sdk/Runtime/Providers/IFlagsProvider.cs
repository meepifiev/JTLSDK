namespace JTLStudio.SDK.Providers
{
    public interface IFlagsProvider : IProvider
    {
        bool TryGetValue(string key, out string value);
    }
}
