using System;
using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK
{
    [Serializable]
    public class ProductDefinition
    {
        [SerializeField] private string _id = "";
        [SerializeField] private ProductType _type = ProductType.NonConsumable;
        [SerializeField] private List<PlatformIdentifier> _platformIds = new List<PlatformIdentifier>();
        [SerializeField] private float _testPrice = 1f;
        [SerializeField] private string _testCurrency = "YAN";

        public ProductDefinition()
        {
        }

        public ProductDefinition(string id, ProductType type, params PlatformIdentifier[] platformIds)
        {
            _id = id;
            _type = type;
            _platformIds.AddRange(platformIds);
        }

        public string Id => _id;
        public ProductType Type => _type;
        public IReadOnlyList<PlatformIdentifier> PlatformIds => _platformIds;
        public float TestPrice => _testPrice;
        public string TestCurrency => _testCurrency;

        public string PlatformIdFor(PlatformId platform)
        {
            foreach (PlatformIdentifier identifier in _platformIds)
            {
                if (identifier.Platform == platform && string.IsNullOrEmpty(identifier.Id) == false)
                {
                    return identifier.Id;
                }
            }

            return _id;
        }
    }
}
