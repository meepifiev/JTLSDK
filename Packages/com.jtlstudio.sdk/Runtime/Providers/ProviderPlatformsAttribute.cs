using System;
using System.Collections.Generic;

namespace JTLStudio.SDK.Providers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ProviderPlatformsAttribute : Attribute
    {
        public ProviderPlatformsAttribute(params PlatformId[] platforms)
        {
            Platforms = platforms ?? Array.Empty<PlatformId>();
        }

        public IReadOnlyList<PlatformId> Platforms { get; }
    }
}
