using System;
using UnityEngine;

namespace JTLStudio.SDK
{
    [Serializable]
    public struct PlatformIdentifier
    {
        [SerializeField] private PlatformId _platform;
        [SerializeField] private string _id;

        public PlatformIdentifier(PlatformId platform, string id)
        {
            _platform = platform;
            _id = id;
        }

        public PlatformId Platform => _platform;
        public string Id => _id;
    }
}
