using System;

namespace JTLStudio.SDK
{
    public interface IAudio : IModule
    {
        float Volume { get; set; }
        bool IsPlatformMuted { get; }

        event Action<bool> PlatformMuteChanged;
    }
}
