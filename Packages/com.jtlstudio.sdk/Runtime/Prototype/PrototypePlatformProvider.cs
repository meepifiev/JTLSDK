#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypePlatformProvider : IPlatformProvider
    {
        private readonly PrototypeSimulationSettings _settings;
        private PrototypeTimer _timer;
        private bool _muted;

        public PrototypePlatformProvider(PlatformId platform, bool supportsPlatformMute, PrototypeSimulationSettings settings)
        {
            Platform = platform;
            SupportsPlatformMute = supportsPlatformMute;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public event Action<bool> PauseRequested;
        public event Action<bool> PlatformMuteChanged;

        public PlatformId Platform { get; }
        public string AppId => "editor";
        public DeviceType DeviceType => _settings.DeviceType;
        public bool SupportsPlatformMute { get; }
        public bool IsPlatformMuted => SupportsPlatformMute && _muted;
        public bool IsPlatformPaused { get; private set; }
        public PrototypeSimulationSettings Settings => _settings;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            PrototypeBridge.RegisterPlatform(this);
            ProviderState state = _settings.SimulateInitializationFailure ? ProviderState.Failed : ProviderState.Ready;
            _timer = new PrototypeTimer(_settings.InitializationDelaySeconds, () => onInitialized(state));
        }

        public void ShowContinuePrompt(Action onContinue)
        {
            onContinue?.Invoke();
        }

        public void SetPlatformPaused(bool paused)
        {
            if (IsPlatformPaused == paused)
            {
                return;
            }

            IsPlatformPaused = paused;
            PauseRequested?.Invoke(paused);
        }

        public void SetPlatformMuted(bool muted)
        {
            if (SupportsPlatformMute == false || _muted == muted)
            {
                return;
            }

            _muted = muted;
            PlatformMuteChanged?.Invoke(muted);
        }

        public void Dispose()
        {
            _timer?.Cancel();
            PrototypeBridge.UnregisterPlatform(this);
        }
    }
}
#endif
