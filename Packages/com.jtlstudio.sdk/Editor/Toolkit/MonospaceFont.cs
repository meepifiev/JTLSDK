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
                element.style.unityFont = _font;

                foreach (VisualElement child in element.Query<VisualElement>().ToList())
                {
                    child.style.unityFont = _font;
                }
            }
        }
    }
}
