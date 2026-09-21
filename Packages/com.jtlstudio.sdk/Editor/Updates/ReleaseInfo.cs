using System.Collections.Generic;

namespace JTLStudio.SDK.Editor.Updates
{
    public class ReleaseInfo
    {
        public ReleaseInfo(string tag, string name, string publishedAt, bool preRelease, IReadOnlyList<string> notes)
        {
            Tag = tag;
            Name = name;
            PublishedAt = publishedAt;
            IsPreRelease = preRelease;
            Notes = notes;
        }

        public string Tag { get; }
        public string Name { get; }
        public string PublishedAt { get; }
        public bool IsPreRelease { get; }
        public IReadOnlyList<string> Notes { get; }
        public string Version => Tag.StartsWith("v") ? Tag.Substring(1) : Tag;
    }
}
