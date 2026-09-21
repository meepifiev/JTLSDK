using System;
using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK
{
    [Serializable]
    public class LeaderboardDefinition
    {
        [SerializeField] private string _id = "";
        [SerializeField] private List<PlatformIdentifier> _platformIds = new List<PlatformIdentifier>();

        public LeaderboardDefinition()
        {
        }

        public LeaderboardDefinition(string id, params PlatformIdentifier[] platformIds)
        {
            _id = id;
            _platformIds.AddRange(platformIds);
        }

        public string Id => _id;
        public IReadOnlyList<PlatformIdentifier> PlatformIds => _platformIds;

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
