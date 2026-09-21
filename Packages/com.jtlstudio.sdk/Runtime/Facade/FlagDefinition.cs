using System;
using UnityEngine;

namespace JTLStudio.SDK
{
    [Serializable]
    public class FlagDefinition
    {
        [SerializeField] private string _key = "";
        [SerializeField] private FlagType _type = FlagType.Bool;
        [SerializeField] private string _defaultValue = "";

        public FlagDefinition()
        {
        }

        public FlagDefinition(string key, FlagType type, string defaultValue)
        {
            _key = key;
            _type = type;
            _defaultValue = defaultValue;
        }

        public string Key => _key;
        public FlagType Type => _type;
        public string DefaultValue => _defaultValue;
    }
}
