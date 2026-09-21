using System;
using System.Collections.Generic;

namespace JTLStudio.SDK
{
    public interface IPause : IModule
    {
        bool IsPaused { get; }
        IReadOnlyCollection<string> Sources { get; }

        event Action<bool> Changed;

        void Set(string source, bool paused);
        IDisposable Hold(string source);
        void ShowContinuePrompt(Action onContinue = null);
    }
}
