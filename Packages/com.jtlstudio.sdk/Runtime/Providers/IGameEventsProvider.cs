namespace JTLStudio.SDK.Providers
{
    public interface IGameEventsProvider : IProvider
    {
        void ReportGameReady();
        void ReportGameplayStart();
        void ReportGameplayStop();
    }
}
