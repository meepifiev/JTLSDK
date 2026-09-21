using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class ColorSwatchField : VisualElement
    {
        public const int DefaultWidth = 140;
        private const string ClassName = "jtl-swatch";
        private const string HexPrefix = "#";

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

        private readonly ColorField _picker = new ColorField { showAlpha = false, showEyeDropper = false };
        private readonly TextField _hex = new TextField { maxLength = 9 };

        public ColorSwatchField()
        {
            AddToClassList(ClassName);
            AddToClassList("jtl-field-box");
            style.width = DefaultWidth;
            _picker.AddToClassList("jtl-swatch__color");
            _hex.AddToClassList("jtl-swatch__hex");
            Add(_picker);
            Add(_hex);
            _picker.RegisterValueChangedCallback(OnPickerChanged);
            _hex.RegisterCallback<FocusOutEvent>(focusEvent => CommitHex());
            _hex.RegisterCallback<KeyDownEvent>(OnHexKeyDown);
        }

        public event Action<Color> ValueChanged;

        public bool ShowAlpha
        {
            get => _picker.showAlpha;
            set
            {
                _picker.showAlpha = value;
                Value = _picker.value;
            }
        }

        public Color Value
        {
            get => _picker.value;
            set
            {
                Color color = ShowAlpha ? value : new Color(value.r, value.g, value.b, 1f);
                _picker.SetValueWithoutNotify(color);
                _hex.SetValueWithoutNotify(ToHex(color));
            }
        }

        public string Hex
        {
            get => ToHex(Value);
            set
            {
                if (ColorUtility.TryParseHtmlString(value, out Color color))
                {
                    Value = color;
                }
            }
        }

        private void OnPickerChanged(ChangeEvent<Color> changeEvent)
        {
            _hex.SetValueWithoutNotify(ToHex(changeEvent.newValue));
            ValueChanged?.Invoke(changeEvent.newValue);
        }

        private void OnHexKeyDown(KeyDownEvent keyEvent)
        {
            if (keyEvent.keyCode == KeyCode.Return || keyEvent.keyCode == KeyCode.KeypadEnter)
            {
                CommitHex();
            }
        }

        private void CommitHex()
        {
            string text = _hex.value.Trim();

            if (text.StartsWith(HexPrefix) == false)
            {
                text = HexPrefix + text;
            }

            if (ColorUtility.TryParseHtmlString(text, out Color color) == false)
            {
                _hex.SetValueWithoutNotify(ToHex(_picker.value));
                return;
            }

            if (ShowAlpha == false)
            {
                color.a = 1f;
            }

            if (color == _picker.value)
            {
                _hex.SetValueWithoutNotify(ToHex(color));
                return;
            }

            _picker.value = color;
        }

        private string ToHex(Color color)
        {
            return HexPrefix + (ShowAlpha && color.a < 0.999f ? ColorUtility.ToHtmlStringRGBA(color) : ColorUtility.ToHtmlStringRGB(color));
        }
    }
}
