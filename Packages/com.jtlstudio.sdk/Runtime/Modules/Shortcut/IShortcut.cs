using System;

namespace JTLStudio.SDK
{
    public interface IShortcut : IModule
    {
        bool CanRequest { get; }

        void Request(Action<bool> onResult);
    }
}
