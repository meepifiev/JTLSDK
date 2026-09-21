using System;
using System.Collections.Generic;
using System.Text;
using JTLStudio.SDK.Editor.Configuration;
using UnityEditor;

namespace JTLStudio.SDK.Editor.Toolkit.Data
{
    public class ProviderCatalog
    {
        private const string ProviderSuffix = "Provider";
        private const string UnsupportedPrefix = "Unsupported";
        private const string FallbackPrefix = "Fallback";
        private const string PrototypeNamespace = "JTLStudio.SDK.Prototype";
        private const string TestsMarker = ".Tests";

        private readonly Dictionary<Type, List<Type>> _cache = new Dictionary<Type, List<Type>>();
        private readonly ProviderPlatformRules _platformRules = new ProviderPlatformRules();

        public IReadOnlyList<Type> ProvidersFor(Type providerInterface)
        {
            if (_cache.TryGetValue(providerInterface, out List<Type> cached))
            {
                return cached;
            }

            List<Type> providers = new List<Type>();

            foreach (Type type in TypeCache.GetTypesDerivedFrom(providerInterface))
            {
                if (IsSelectable(type))
                {
                    providers.Add(type);
                }
            }

            providers.Sort((left, right) => Order(left).CompareTo(Order(right)) != 0 ? Order(left).CompareTo(Order(right)) : string.CompareOrdinal(left.Name, right.Name));
            _cache[providerInterface] = providers;
            return providers;
        }

        public IReadOnlyList<Type> ProvidersFor(Type providerInterface, PlatformId platform)
        {
            List<Type> available = new List<Type>();

            foreach (Type type in ProvidersFor(providerInterface))
            {
                if (_platformRules.IsAvailable(type, platform))
                {
                    available.Add(type);
                }
            }

            return available;
        }

        public string DisplayName(Type providerType)
        {
            if (providerType == null)
            {
                return "None";
            }

            string name = providerType.Name;

            if (name.StartsWith(UnsupportedPrefix))
            {
                return "Unsupported";
            }

            if (name.StartsWith(FallbackPrefix))
            {
                return "Built-in";
            }

            if (name.EndsWith(ProviderSuffix))
            {
                name = name.Substring(0, name.Length - ProviderSuffix.Length);
            }

            return SplitWords(name);
        }

        public bool IsUnsupported(Type providerType)
        {
            return providerType != null && providerType.Name.StartsWith(UnsupportedPrefix);
        }

        private bool IsSelectable(Type type)
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
            {
                return false;
            }

            if (type.Namespace == PrototypeNamespace || type.Assembly.GetName().Name.Contains(TestsMarker))
            {
                return false;
            }

            return type.IsSerializable && type.GetConstructor(Type.EmptyTypes) != null;
        }

        private int Order(Type type)
        {
            if (type.Name.StartsWith(UnsupportedPrefix))
            {
                return 2;
            }

            if (type.Name.StartsWith(FallbackPrefix))
            {
                return 1;
            }

            return 0;
        }

        private string SplitWords(string name)
        {
            StringBuilder builder = new StringBuilder(name.Length + 8);

            for (int index = 0; index < name.Length; index++)
            {
                char current = name[index];
                bool boundary = index > 0 && char.IsUpper(current) && (char.IsLower(name[index - 1]) || (index + 1 < name.Length && char.IsLower(name[index + 1])));

                if (boundary)
                {
                    builder.Append(' ');
                }

                builder.Append(current);
            }

            return builder.ToString().Replace("You Tube", "YouTube");
        }
    }
}
