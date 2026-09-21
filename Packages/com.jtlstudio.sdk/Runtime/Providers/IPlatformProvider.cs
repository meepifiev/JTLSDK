using System;

namespace JTLStudio.SDK.Providers
{
    public interface IPlatformProvider : IProvider
    {
        PlatformId Platform { get; }
        string AppId { get; }
        DeviceType DeviceType { get; }
        bool SupportsPlatformMute { get; }
        bool IsPlatformMuted { get; }

        event Action<bool> PauseRequested;
        event Action<bool> PlatformMuteChanged;

        void ShowContinuePrompt(Action onContinue);
    }
}
