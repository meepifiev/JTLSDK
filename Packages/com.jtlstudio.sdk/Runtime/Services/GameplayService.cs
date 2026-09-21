using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class GameplayService : ModuleBase, IGameplay
    {
        private readonly IGameplayProvider _provider;
        private readonly PauseService _pause;
        private bool _gameReadyRequested;
        private bool _wasPlayingBeforeSuspend;
        private int _suspendDepth;

        public GameplayService(IGameplayProvider provider, PauseService pause, SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
            _pause.Changed += OnPauseChanged;
        }

        public bool IsGameReady { get; private set; }
        public bool IsPlaying { get; private set; }

        internal override string ModuleName => "Gameplay";

        public void GameReady()
        {
            if (IsGameReady)
            {
                return;
            }

            _gameReadyRequested = true;

            if (IsReady)
            {
                SendGameReady();
            }
        }

        public void Start()
        {
            if (IsPlaying)
            {
                return;
            }

            IsPlaying = true;
            Report(_provider.ReportGameplayStart);
        }

        public void Stop()
        {
            if (IsPlaying == false)
            {
                return;
            }

            IsPlaying = false;
            Report(_provider.ReportGameplayStop);
        }

        internal override void Initialize()
        {
            _provider.Initialize(OnProviderInitialized);
        }

        internal override void Dispose()
        {
            _pause.Changed -= OnPauseChanged;
        }

        internal void Suspend()
        {
            _suspendDepth++;

            if (_suspendDepth > 1)
            {
                return;
            }

            _wasPlayingBeforeSuspend = IsPlaying;
            Stop();
        }

        internal void Resume()
        {
            if (_suspendDepth == 0)
            {
                return;
            }

            _suspendDepth--;

            if (_suspendDepth == 0 && _wasPlayingBeforeSuspend)
            {
                Start();
            }
        }

        private void OnProviderInitialized(ProviderState state)
        {
            CompleteInitialization(state);

            if (_gameReadyRequested)
            {
                SendGameReady();
            }

            if (IsPlaying)
            {
                Report(_provider.ReportGameplayStart);
            }
        }

        private void SendGameReady()
        {
            IsGameReady = true;
            Report(_provider.ReportGameReady);
        }

        private void Report(Action report)
        {
            if (State != ModuleState.Ready)
            {
                return;
            }

            try
            {
                report();
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
            }
        }

        private void OnPauseChanged(bool paused)
        {
            if (paused)
            {
                Suspend();
            }
            else
            {
                Resume();
            }
        }
    }
}
