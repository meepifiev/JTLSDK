using System;

namespace JTLStudio.SDK.Providers
{
    public interface IGameLabelProvider : IProvider
    {
        bool CanShow { get; }

        void ShowDialog(Action<bool> onResult);
    }
}
