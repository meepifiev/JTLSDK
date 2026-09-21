using System;

namespace JTLStudio.SDK
{
    public interface IGameLabel : IModule
    {
        bool CanShow { get; }

        void ShowDialog(Action<bool> onResult);
    }
}
