namespace JTLStudio.SDK.Providers
{
    public interface IGameplayProvider : IProvider
    {
        void ReportGameReady();
        void ReportGameplayStart();
        void ReportGameplayStop();
    }
}
