using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit
{
    public class ToolkitAssets
    {
        public const string Root = "Packages/com.jtlstudio.sdk/Editor/Toolkit/";

        public VisualTreeAsset FindTemplate(string relativePath)
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Root + relativePath);
        }

        public StyleSheet FindStyleSheet(string relativePath)
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(Root + relativePath);
        }

        public VisualTreeAsset LoadTemplate(string relativePath)
        {
            VisualTreeAsset template = FindTemplate(relativePath);

            if (template == null)
            {
                throw new InvalidOperationException(nameof(relativePath));
            }

            return template;
        }

        public StyleSheet LoadStyleSheet(string relativePath)
        {
            StyleSheet styleSheet = FindStyleSheet(relativePath);

            if (styleSheet == null)
            {
                throw new InvalidOperationException(nameof(relativePath));
            }

            return styleSheet;
        }
    }
}
