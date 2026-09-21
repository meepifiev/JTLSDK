using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Editor.Toolkit.Icons
{
    public class IconLibrary
    {
        public const string Root = "Packages/com.jtlstudio.sdk/Editor/Toolkit/Icons/";
        private const int SmallSize = 16;
        private const int MediumSize = 20;
        private const int LargeSize = 24;
        private const float RetinaThreshold = 1.5f;
        private const string RetinaSuffix = "@2x";

        private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();

        public Texture2D Load(string iconName, int logicalSize)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return null;
            }

            string path = BuildPath(iconName, logicalSize);

            if (_cache.TryGetValue(path, out Texture2D cached) && cached != null)
            {
                return cached;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            _cache[path] = texture;
            return texture;
        }

        public string BuildPath(string iconName, int logicalSize)
        {
            int textureSize = ResolveTextureSize(logicalSize);
            string suffix = EditorGUIUtility.pixelsPerPoint > RetinaThreshold ? RetinaSuffix : string.Empty;
            return Root + iconName + "-" + textureSize + suffix + ".png";
        }

        private int ResolveTextureSize(int logicalSize)
        {
            if (logicalSize <= SmallSize)
            {
                return SmallSize;
            }

            if (logicalSize <= MediumSize)
            {
                return MediumSize;
            }

            return LargeSize;
        }
    }
}
