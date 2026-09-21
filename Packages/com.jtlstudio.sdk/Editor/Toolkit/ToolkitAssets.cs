using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit
{
    public class ToolkitAssets
    {
        public const string Root = "Packages/com.jtlstudio.sdk/Editor/Toolkit/";

        public VisualTreeAsset LoadTemplate(string relativePath)
        {
            VisualTreeAsset template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Root + relativePath);

            if (template == null)
            {
                throw new InvalidOperationException(nameof(relativePath));
            }

            return template;
        }

        public StyleSheet LoadStyleSheet(string relativePath)
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Root + relativePath);

            if (styleSheet == null)
            {
                throw new InvalidOperationException(nameof(relativePath));
            }

            return styleSheet;
        }
    }
}
