using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class FallbackPlatformProvider : IPlatformProvider
    {
        public event Action<bool> PauseRequested;
        public event Action<bool> PlatformMuteChanged;

        public PlatformId Platform => PlatformId.Editor;
        public string AppId => "";
        public DeviceType DeviceType => DeviceType.Desktop;
        public bool SupportsPlatformMute => false;
        public bool IsPlatformMuted => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void ShowContinuePrompt(Action onContinue)
        {
            onContinue?.Invoke();
        }

        internal void RequestPause(bool paused)
        {
            PauseRequested?.Invoke(paused);
        }

        internal void ChangePlatformMute(bool muted)
        {
            PlatformMuteChanged?.Invoke(muted);
        }
    }
}
