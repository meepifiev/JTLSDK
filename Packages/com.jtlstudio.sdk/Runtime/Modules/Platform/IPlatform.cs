namespace JTLStudio.SDK
{
    public interface IPlatform : IModule
    {
        PlatformId Current { get; }
        string AppId { get; }

        bool Supports(Capability capability);
    }
}
