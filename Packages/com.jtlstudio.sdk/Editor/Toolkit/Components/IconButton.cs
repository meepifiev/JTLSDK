using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class IconButton : Button
    {
        public const string GhostVariant = "ghost";
        public const string SecondaryVariant = "secondary";
        public const int DefaultSize = 30;
        private const string ClassName = "jtl-icon-button";
        private const string VariantPrefix = "jtl-icon-button--";

        public new class UxmlFactory : UxmlFactory<IconButton, UxmlTraits>
        {
        }

        public new class UxmlTraits : Button.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _icon = new UxmlStringAttributeDescription { name = "icon" };
            private readonly UxmlStringAttributeDescription _variant = new UxmlStringAttributeDescription { name = "variant", defaultValue = GhostVariant };
            private readonly UxmlIntAttributeDescription _size = new UxmlIntAttributeDescription { name = "size", defaultValue = DefaultSize };
            private readonly UxmlIntAttributeDescription _iconSize = new UxmlIntAttributeDescription { name = "icon-size", defaultValue = Icon.DefaultSize };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                IconButton button = (IconButton)element;
                button.Variant = _variant.GetValueFromBag(attributes, context);
                button.Size = _size.GetValueFromBag(attributes, context);
                button.IconSize = _iconSize.GetValueFromBag(attributes, context);
                button.IconName = _icon.GetValueFromBag(attributes, context);
                button.text = string.Empty;
            }
        }

        private readonly Icon _icon = new Icon();
        private string _variant = GhostVariant;
        private int _size = DefaultSize;

        public IconButton()
        {
            AddToClassList(ClassName);
            AddToClassList(VariantPrefix + GhostVariant);
            _icon.AddToClassList("jtl-icon-button__icon");
            Add(_icon);
            text = string.Empty;
            ApplySize();
        }

        public IconButton(string iconName, int size) : this()
        {
            Size = size;
            IconName = iconName;
        }

        public string IconName
        {
            get => _icon.IconName;
            set => _icon.IconName = value;
        }

        public int IconSize
        {
            get => _icon.Size;
            set => _icon.Size = value;
        }

        public string Variant
        {
            get => _variant;
            set
            {
                RemoveFromClassList(VariantPrefix + _variant);
                _variant = string.IsNullOrEmpty(value) ? GhostVariant : value;
                AddToClassList(VariantPrefix + _variant);
                _icon.Tone = _variant == SecondaryVariant ? "text" : Icon.DefaultTone;
            }
        }

        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                ApplySize();
            }
        }

        private void ApplySize()
        {
            style.width = _size;
            style.height = _size;
            style.minWidth = _size;
            style.minHeight = _size;
        }
    }
}
