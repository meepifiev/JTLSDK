using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class ToolkitButton : Button, ILocalizedElement
    {
        public const string PrimaryVariant = "primary";
        public const string SecondaryVariant = "secondary";
        public const string GhostVariant = "ghost";
        public const string DangerVariant = "danger";
        public const string DropdownVariant = "dropdown";
        private const string ClassName = "jtl-button";
        private const string VariantPrefix = "jtl-button--";
        private const string CompactClass = "jtl-button--compact";
        private const string BlockClass = "jtl-button--block";
        private const string OnAccentTone = "on-accent";
        private const string TextTone = "text";
        private const string MutedTone = "muted";

        public new class UxmlFactory : UxmlFactory<ToolkitButton, UxmlTraits>
        {
        }

        public new class UxmlTraits : Button.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _textKey = new UxmlStringAttributeDescription { name = "text-key" };
            private readonly UxmlStringAttributeDescription _icon = new UxmlStringAttributeDescription { name = "icon" };
            private readonly UxmlStringAttributeDescription _trailingIcon = new UxmlStringAttributeDescription { name = "trailing-icon" };
            private readonly UxmlStringAttributeDescription _variant = new UxmlStringAttributeDescription { name = "variant", defaultValue = SecondaryVariant };
            private readonly UxmlBoolAttributeDescription _compact = new UxmlBoolAttributeDescription { name = "compact" };
            private readonly UxmlBoolAttributeDescription _block = new UxmlBoolAttributeDescription { name = "block" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                ToolkitButton button = (ToolkitButton)element;
                button.Variant = _variant.GetValueFromBag(attributes, context);
                button.Compact = _compact.GetValueFromBag(attributes, context);
                button.Block = _block.GetValueFromBag(attributes, context);
                button.IconName = _icon.GetValueFromBag(attributes, context);
                button.TrailingIconName = _trailingIcon.GetValueFromBag(attributes, context);
                button.TextKey = _textKey.GetValueFromBag(attributes, context);
                button.Label = button.text;
                button.text = string.Empty;
            }
        }

        private readonly Icon _leadingIcon = new Icon();
        private readonly Label _label = new Label();
        private readonly Icon _trailingIcon = new Icon();
        private string _variant = SecondaryVariant;
        private string _labelText;

        public ToolkitButton()
        {
            AddToClassList(ClassName);
            AddToClassList(VariantPrefix + SecondaryVariant);
            _leadingIcon.AddToClassList("jtl-button__icon");
            _label.AddToClassList("jtl-button__label");
            _trailingIcon.AddToClassList("jtl-button__trailing");
            _leadingIcon.style.display = DisplayStyle.None;
            _trailingIcon.style.display = DisplayStyle.None;
            _label.style.display = DisplayStyle.None;
            Add(_leadingIcon);
            Add(_label);
            Add(_trailingIcon);
            text = string.Empty;
        }

        public ToolkitButton(string textKey, string variant) : this()
        {
            TextKey = textKey;
            Variant = variant;
        }

        public string TextKey { get; set; }

        public string Label
        {
            get => _labelText;
            set
            {
                _labelText = value;
                _label.text = value;
                _label.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        public string Variant
        {
            get => _variant;
            set
            {
                RemoveFromClassList(VariantPrefix + _variant);
                _variant = string.IsNullOrEmpty(value) ? SecondaryVariant : value;
                AddToClassList(VariantPrefix + _variant);
                ApplyIconTone();
            }
        }

        public bool Compact
        {
            get => ClassListContains(CompactClass);
            set => EnableInClassList(CompactClass, value);
        }

        public bool Block
        {
            get => ClassListContains(BlockClass);
            set => EnableInClassList(BlockClass, value);
        }

        public string IconName
        {
            get => _leadingIcon.IconName;
            set => _leadingIcon.IconName = value;
        }

        public string TrailingIconName
        {
            get => _trailingIcon.IconName;
            set => _trailingIcon.IconName = value;
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(TextKey) == false)
            {
                Label = localization.Get(TextKey);
            }
        }

        private void ApplyIconTone()
        {
            string tone = ResolveIconTone();
            _leadingIcon.Tone = tone;
            _trailingIcon.Tone = _variant == DropdownVariant ? MutedTone : tone;
        }

        private string ResolveIconTone()
        {
            if (_variant == PrimaryVariant || _variant == DangerVariant)
            {
                return OnAccentTone;
            }

            if (_variant == GhostVariant)
            {
                return Icon.DefaultTone;
            }

            return TextTone;
        }
    }
}
