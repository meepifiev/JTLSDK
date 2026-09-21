using System;
using System.Collections.Generic;
using System.Globalization;
using JTLStudio.SDK.Services.Json;

namespace JTLStudio.SDK.Services
{
    public class DataDocument
    {
        public const int CurrentFormat = 1;

        private const string FormatKey = "format";
        private const string RevisionKey = "revision";
        private const string SavedAtKey = "savedAt";
        private const string ValuesKey = "values";

        private readonly JsonReader _reader = new JsonReader();
        private readonly JsonWriter _writer = new JsonWriter();

        public long Revision { get; private set; }
        public string SavedAt { get; private set; } = "";
        public Dictionary<string, object> Values { get; private set; } = new Dictionary<string, object>();

        public bool TryParse(string serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return false;
            }

            object parsed;

            try
            {
                parsed = _reader.Parse(serialized);
            }
            catch (FormatException)
            {
                return false;
            }

            if (parsed is Dictionary<string, object> root == false)
            {
                return false;
            }

            Revision = root.TryGetValue(RevisionKey, out object revision) && revision is long revisionNumber ? revisionNumber : 0;
            SavedAt = root.TryGetValue(SavedAtKey, out object savedAt) && savedAt is string savedAtText ? savedAtText : "";
            Values = root.TryGetValue(ValuesKey, out object values) && values is Dictionary<string, object> valueMap ? valueMap : new Dictionary<string, object>();
            return true;
        }

        public string Serialize()
        {
            Revision++;
            SavedAt = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

            Dictionary<string, object> root = new Dictionary<string, object>
            {
                [FormatKey] = CurrentFormat,
                [RevisionKey] = Revision,
                [SavedAtKey] = SavedAt,
                [ValuesKey] = Values
            };

            return _writer.Write(root);
        }

        public string SerializeValue(object value)
        {
            return _writer.Write(value);
        }

        public object ParseValue(string serialized)
        {
            return _reader.Parse(serialized);
        }

        public void Replace(DataDocument other)
        {
            Revision = other.Revision;
            SavedAt = other.SavedAt;
            Values = other.Values;
        }

        public void Clear()
        {
            Values = new Dictionary<string, object>();
        }
    }
}
