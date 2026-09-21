namespace JTLStudio.SDK.Providers
{
    public interface IAnalyticsProvider : IProvider
    {
        void Report(string eventName, string parametersJson);
    }
}
