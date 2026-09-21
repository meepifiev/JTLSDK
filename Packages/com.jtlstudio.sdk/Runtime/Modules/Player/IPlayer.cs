using System;

namespace JTLStudio.SDK
{
    public interface IPlayer : IModule
    {
        bool IsAuthorized { get; }
        string Id { get; }
        string Name { get; }
        string AvatarUrl { get; }

        event Action Authorized;

        void Authorize(Action<bool> onResult);
    }
}
