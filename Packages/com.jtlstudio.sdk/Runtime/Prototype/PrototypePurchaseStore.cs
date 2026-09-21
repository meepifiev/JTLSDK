#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK.Prototype
{
    [Serializable]
    public class PrototypePurchaseStore
    {
        public const string StorageKey = "JTLSDK.Prototype.Purchases";

        [SerializeField] private List<PrototypePurchaseRecord> _records = new List<PrototypePurchaseRecord>();

        public List<PrototypePurchaseRecord> Records => _records;

        public void Load()
        {
            string stored = PlayerPrefs.GetString(StorageKey, "");

            if (string.IsNullOrEmpty(stored))
            {
                _records = new List<PrototypePurchaseRecord>();
                return;
            }

            JsonUtility.FromJsonOverwrite(stored, this);
            _records ??= new List<PrototypePurchaseRecord>();
        }

        public void Save()
        {
            PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            _records.Clear();
            PlayerPrefs.DeleteKey(StorageKey);
            PlayerPrefs.Save();
        }
    }
}
#endif
