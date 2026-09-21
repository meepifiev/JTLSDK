using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class Badge : VisualElement, ILocalizedElement
    {
        public const string NeutralVariant = "neutral";
        public const string SuccessVariant = "success";
        public const string WarningVariant = "warning";
        public const string ErrorVariant = "error";
        public const string AccentVariant = "accent";
        private const string ClassName = "jtl-badge";
        private const string VariantPrefix = "jtl-badge--";

        public new class UxmlFactory : UxmlFactory<Badge, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _textKey = new UxmlStringAttributeDescription { name = "text-key" };
            private readonly UxmlStringAttributeDescription _text = new UxmlStringAttributeDescription { name = "text" };
            private readonly UxmlStringAttributeDescription _variant = new UxmlStringAttributeDescription { name = "variant", defaultValue = NeutralVariant };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                Badge badge = (Badge)element;
                badge.Variant = _variant.GetValueFromBag(attributes, context);
                badge.Text = _text.GetValueFromBag(attributes, context);
                badge.TextKey = _textKey.GetValueFromBag(attributes, context);
            }
        }

        private readonly Label _label = new Label();
        private string _variant = NeutralVariant;

        public Badge()
        {
            AddToClassList(ClassName);
            AddToClassList(VariantPrefix + NeutralVariant);
            _label.AddToClassList("jtl-badge__label");
            _label.pickingMode = PickingMode.Ignore;
            Add(_label);
        }

        public Badge(string textKey, string variant) : this()
        {
            TextKey = textKey;
            Variant = variant;
        }

        public string TextKey { get; set; }

        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        public string Variant
        {
            get => _variant;
            set
            {
                RemoveFromClassList(VariantPrefix + _variant);
                _variant = string.IsNullOrEmpty(value) ? NeutralVariant : value;
                AddToClassList(VariantPrefix + _variant);
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(TextKey) == false)
            {
                Text = localization.Get(TextKey);
            }
        }
    }
}
