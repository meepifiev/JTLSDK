using System;

namespace JTLStudio.SDK
{
    public interface ITime : IModule
    {
        float Scale { get; set; }
        DateTimeOffset Now { get; }
        bool IsServerTime { get; }
    }
}
