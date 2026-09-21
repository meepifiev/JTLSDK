using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Editor.Build
{
    [FilePath("UserSettings/JTLSDKBuildHistory.asset", FilePathAttribute.Location.ProjectFolder)]
    public class BuildHistory : ScriptableSingleton<BuildHistory>
    {
        public const int Capacity = 20;

        [SerializeField] private List<BuildRecord> _records = new List<BuildRecord>();

        public IReadOnlyList<BuildRecord> Records => _records;

        public void Add(BuildRecord record)
        {
            _records.Insert(0, record);

            if (_records.Count > Capacity)
            {
                _records.RemoveRange(Capacity, _records.Count - Capacity);
            }

            Save(true);
        }

        public void Clear()
        {
            _records.Clear();
            Save(true);
        }
    }
}
