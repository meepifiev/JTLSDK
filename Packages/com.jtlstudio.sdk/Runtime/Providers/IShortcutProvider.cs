using System;

namespace JTLStudio.SDK.Providers
{
    public interface IShortcutProvider : IProvider
    {
        bool CanRequest { get; }

        void Request(Action<bool> onResult);
    }
}
