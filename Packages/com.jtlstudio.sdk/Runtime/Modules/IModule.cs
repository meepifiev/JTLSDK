using System;

namespace JTLStudio.SDK
{
    public interface IModule
    {
        ModuleState State { get; }
        bool IsSupported { get; }
        bool IsReady { get; }

        void WhenReady(Action onReady);
    }
}
