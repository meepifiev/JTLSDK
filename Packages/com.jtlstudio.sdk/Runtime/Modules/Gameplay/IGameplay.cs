namespace JTLStudio.SDK
{
    public interface IGameplay : IModule
    {
        bool IsGameReady { get; }
        bool IsPlaying { get; }

        void GameReady();
        void Start();
        void Stop();
    }
}
