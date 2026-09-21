using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class ColorSwatchField : VisualElement
    {
        public const int DefaultWidth = 120;
        private const string ClassName = "jtl-swatch";

        public new class UxmlFactory : UxmlFactory<ColorSwatchField, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _color = new UxmlStringAttributeDescription { name = "color", defaultValue = "#000000" };
            private readonly UxmlIntAttributeDescription _width = new UxmlIntAttributeDescription { name = "width", defaultValue = DefaultWidth };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                ColorSwatchField field = (ColorSwatchField)element;
                field.Hex = _color.GetValueFromBag(attributes, context);
                field.style.width = _width.GetValueFromBag(attributes, context);
            }
        }

        private readonly VisualElement _swatch = new VisualElement();
        private readonly Label _hex = new Label();
        private string _value;

        public ColorSwatchField()
        {
            AddToClassList(ClassName);
            AddToClassList("jtl-field-box");
            _swatch.AddToClassList("jtl-swatch__color");
            _hex.AddToClassList("jtl-swatch__hex");
            Add(_swatch);
            Add(_hex);
        }

        public string Hex
        {
            get => _value;
            set
            {
                _value = value;
                _hex.text = value;

                if (ColorUtility.TryParseHtmlString(value, out Color color))
                {
                    _swatch.style.backgroundColor = color;
                }
            }
        }
    }
}
