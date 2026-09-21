using System.Collections.Generic;
using JTLStudio.SDK.Services.Json;

namespace JTLStudio.SDK.Bridge
{
    public class BridgePayload
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        private readonly JsonWriter _writer = new JsonWriter();

        public BridgePayload Set(string key, string value)
        {
            _values[key] = value ?? "";
            return this;
        }

        public BridgePayload Set(string key, long value)
        {
            _values[key] = value;
            return this;
        }

        public BridgePayload Set(string key, bool value)
        {
            _values[key] = value;
            return this;
        }

        public BridgePayload Set(string key, Dictionary<string, object> value)
        {
            _values[key] = value;
            return this;
        }

        public override string ToString()
        {
            return _writer.Write(_values);
        }
    }
}
