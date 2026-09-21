using System;

namespace JTLStudio.SDK.Providers
{
    public interface ITimeProvider : IProvider
    {
        bool IsServerTime { get; }
        DateTimeOffset Now { get; }
    }
}
