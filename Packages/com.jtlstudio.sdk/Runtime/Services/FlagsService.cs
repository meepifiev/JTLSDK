using System;
using System.Collections.Generic;
using System.Globalization;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class FlagsService : ModuleBase, IFlags
    {
        private readonly IFlagsProvider _provider;
        private readonly IReadOnlyList<FlagDefinition> _definitions;

        public FlagsService(IFlagsProvider provider, IReadOnlyList<FlagDefinition> definitions, SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        internal override string ModuleName => "Flags";

        public bool HasKey(string key)
        {
            return TryGetRaw(key, out _);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return TryGetRaw(key, out string raw) && bool.TryParse(raw, out bool value) ? value : defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return TryGetRaw(key, out string raw) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            return TryGetRaw(key, out string raw) && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : defaultValue;
        }

        public string GetString(string key, string defaultValue = "")
        {
            return TryGetRaw(key, out string raw) ? raw : defaultValue;
        }

        internal override void Initialize()
        {
            _provider.Initialize(CompleteInitialization);
        }

        private bool TryGetRaw(string key, out string raw)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }

            if (State == ModuleState.Ready && _provider.TryGetValue(key, out raw))
            {
                return true;
            }

            foreach (FlagDefinition definition in _definitions)
            {
                if (definition.Key == key)
                {
                    raw = definition.DefaultValue;
                    return true;
                }
            }

            raw = "";
            return false;
        }
    }
}
