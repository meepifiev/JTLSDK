using System;
using System.Collections.Generic;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesFlagsProvider : BridgeProviderBase, IFlagsProvider
    {
        private readonly Dictionary<string, string> _flags = new Dictionary<string, string>();

        public void Initialize(Action<ProviderState> onInitialized)
        {
            InitializeWith("flags", "get", onInitialized, response =>
            {
                _flags.Clear();

                foreach (KeyValuePair<string, object> pair in AsObject(response.Values.TryGetValue("flags", out object flags) ? flags : null))
                {
                    _flags[pair.Key] = Convert.ToString(pair.Value, System.Globalization.CultureInfo.InvariantCulture);
                }
            });
        }

        public bool TryGetValue(string key, out string value)
        {
            return _flags.TryGetValue(key, out value);
        }
    }
}
