using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class InlineMessage : VisualElement, ILocalizedElement
    {
        public const string ErrorVariant = "error";
        public const string WarningVariant = "warning";
        public const string InfoVariant = "info";
        private const int IconSize = 13;
        private const string ClassName = "jtl-message";
        private const string VariantPrefix = "jtl-message--";

        public new class UxmlFactory : UxmlFactory<InlineMessage, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _textKey = new UxmlStringAttributeDescription { name = "text-key" };
            private readonly UxmlStringAttributeDescription _variant = new UxmlStringAttributeDescription { name = "variant", defaultValue = InfoVariant };
            private readonly UxmlIntAttributeDescription _indent = new UxmlIntAttributeDescription { name = "indent" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                InlineMessage message = (InlineMessage)element;
                message.Variant = _variant.GetValueFromBag(attributes, context);
                message.TextKey = _textKey.GetValueFromBag(attributes, context);
                message.style.paddingLeft = _indent.GetValueFromBag(attributes, context);
            }
        }

        private readonly Icon _icon = new Icon();
        private readonly Label _text = new Label();
        private string _variant = InfoVariant;

        public InlineMessage()
        {
            AddToClassList(ClassName);
            AddToClassList(VariantPrefix + InfoVariant);
            _icon.AddToClassList("jtl-message__icon");
            _icon.Size = IconSize;
            _text.AddToClassList("jtl-message__text");
            Add(_icon);
            Add(_text);
            ApplyIcon();
        }

        public InlineMessage(string textKey, string variant) : this()
        {
            TextKey = textKey;
            Variant = variant;
        }

        public string TextKey { get; set; }

        public string Text
        {
            get => _text.text;
            set => _text.text = value;
        }

        public string Variant
        {
            get => _variant;
            set
            {
                RemoveFromClassList(VariantPrefix + _variant);
                _variant = string.IsNullOrEmpty(value) ? InfoVariant : value;
                AddToClassList(VariantPrefix + _variant);
                ApplyIcon();
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(TextKey) == false)
            {
                _text.text = localization.Get(TextKey);
            }
        }

        private void ApplyIcon()
        {
            if (_variant == InfoVariant)
            {
                _icon.IconName = "info";
                _icon.Tone = "muted";
                return;
            }

            _icon.IconName = "alert-triangle";
            _icon.Tone = _variant;
        }
    }
}
