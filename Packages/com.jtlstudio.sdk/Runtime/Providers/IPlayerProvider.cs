using System;

namespace JTLStudio.SDK.Providers
{
    public interface IPlayerProvider : IProvider
    {
        bool IsAuthorized { get; }
        string Id { get; }
        string Name { get; }
        string AvatarUrl { get; }

        void Authorize(Action<bool> onResult);
    }
}
