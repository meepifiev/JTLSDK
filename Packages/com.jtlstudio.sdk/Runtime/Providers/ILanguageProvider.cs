namespace JTLStudio.SDK.Providers
{
    public interface ILanguageProvider : IProvider
    {
        string LanguageCode { get; }
    }
}
