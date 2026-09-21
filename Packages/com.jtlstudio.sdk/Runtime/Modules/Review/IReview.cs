using System;

namespace JTLStudio.SDK
{
    public interface IReview : IModule
    {
        bool CanRequest { get; }

        void Request(Action<bool> onResult);
    }
}
