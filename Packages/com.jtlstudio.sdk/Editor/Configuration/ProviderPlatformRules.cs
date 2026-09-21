using System;
using System.Reflection;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class ProviderPlatformRules
    {
        public bool IsAvailable(Type providerType, PlatformId platform)
        {
            if (providerType == null)
            {
                return true;
            }

            ProviderPlatformsAttribute attribute = providerType.GetCustomAttribute<ProviderPlatformsAttribute>();

            if (attribute == null)
            {
                return true;
            }

            foreach (PlatformId supported in attribute.Platforms)
            {
                if (supported == platform)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
