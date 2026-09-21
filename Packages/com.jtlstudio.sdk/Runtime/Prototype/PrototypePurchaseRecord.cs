#if UNITY_EDITOR
using System;
using UnityEngine;

namespace JTLStudio.SDK.Prototype
{
    [Serializable]
    public class PrototypePurchaseRecord
    {
        [SerializeField] private string _productId = "";
        [SerializeField] private string _token = "";
        [SerializeField] private bool _consumed;

        public PrototypePurchaseRecord()
        {
        }

        public PrototypePurchaseRecord(string productId, string token)
        {
            _productId = productId;
            _token = token;
        }

        public string ProductId => _productId;
        public string Token => _token;

        public bool Consumed
        {
            get => _consumed;
            set => _consumed = value;
        }
    }
}
#endif
