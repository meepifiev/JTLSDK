using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit
{
    public class MonospaceFont
    {
        public const string ClassName = "jtl-mono";
        private const string FontPath = "Fonts/RobotoMono/RobotoMono-Regular.ttf";

        private Font _font;

        public void Apply(VisualElement root)
        {
            if (_font == null)
            {
                _font = EditorGUIUtility.Load(FontPath) as Font;
            }

            if (_font == null)
            {
                return;
            }

            List<VisualElement> elements = root.Query<VisualElement>(className: ClassName).ToList();

            foreach (VisualElement element in elements)
            {
                if (element is TextField field && field.multiline == false)
                {
                    continue;
                }

                foreach (VisualElement target in element.Query<VisualElement>().ToList())
                {
                    target.style.unityFont = _font;
                    target.style.unityFontDefinition = FontDefinition.FromFont(_font);
                }
            }
        }
    }
}
