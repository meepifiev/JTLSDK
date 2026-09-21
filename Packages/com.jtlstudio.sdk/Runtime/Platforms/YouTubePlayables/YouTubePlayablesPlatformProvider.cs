using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    public class YouTubePlayablesPlatformProvider : IPlatformProvider
    {
        public event Action<bool> PauseRequested;
        public event Action<bool> PlatformMuteChanged;

        public PlatformId Platform => PlatformId.YouTubePlayables;
        public string AppId => "";
        public DeviceType DeviceType => DeviceType.Desktop;
        public bool SupportsPlatformMute => true;
        public bool IsPlatformMuted => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }

        public void ShowContinuePrompt(Action onContinue)
        {
            onContinue?.Invoke();
        }

        internal void RaisePause(bool paused)
        {
            PauseRequested?.Invoke(paused);
        }

        internal void RaiseMute(bool muted)
        {
            PlatformMuteChanged?.Invoke(muted);
        }
    }
}
