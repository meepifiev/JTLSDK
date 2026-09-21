using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests
{
    public class FakePlatformProvider : IPlatformProvider
    {
        private Action<ProviderState> _onInitialized;

        public event Action<bool> PauseRequested;
        public event Action<bool> PlatformMuteChanged;

        public PlatformId Platform { get; set; } = PlatformId.YandexGames;
        public string AppId { get; set; } = "test-app";
        public DeviceType DeviceType { get; set; } = DeviceType.Desktop;
        public bool SupportsPlatformMute { get; set; }
        public bool IsPlatformMuted { get; set; }
        public bool IsInitializing => _onInitialized != null;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            _onInitialized = onInitialized;
        }

        public void ShowContinuePrompt(Action onContinue)
        {
            onContinue?.Invoke();
        }

        public void Complete(ProviderState state)
        {
            Action<ProviderState> callback = _onInitialized;
            _onInitialized = null;
            callback?.Invoke(state);
        }

        public void RequestPause(bool paused)
        {
            PauseRequested?.Invoke(paused);
        }

        public void SetPlatformMuted(bool muted)
        {
            IsPlatformMuted = muted;
            PlatformMuteChanged?.Invoke(muted);
        }
    }
}
