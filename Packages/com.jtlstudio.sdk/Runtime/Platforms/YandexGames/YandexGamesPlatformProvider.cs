using System;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesPlatformProvider : IPlatformProvider
    {
        [SerializeField] private string _appId = "";

        public event Action<bool> PauseRequested;
        public event Action<bool> PlatformMuteChanged;

        public PlatformId Platform => PlatformId.YandexGames;
        public string AppId => _appId;
        public DeviceType DeviceType => DeviceType.Desktop;
        public bool SupportsPlatformMute => false;
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
