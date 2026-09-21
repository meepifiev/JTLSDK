namespace JTLStudio.SDK
{
    public interface IGameEvents : IModule
    {
        bool IsGameReady { get; }
        bool IsGameplayActive { get; }

        void GameReady();
        void GameplayStarted();
        void GameplayRestarted();
        void GameplayStopped();
    }
}
