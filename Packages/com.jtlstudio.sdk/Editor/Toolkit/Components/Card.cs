using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class Card : VisualElement, ILocalizedElement
    {
        public const int DefaultSpacing = 12;
        private const int HeaderGap = 12;
        private const string ClassName = "jtl-card";
        private const string ActiveClass = "jtl-card--active";
        private const string DimmedClass = "jtl-card--dimmed";
        private const string StackPrefix = "jtl-vstack-";

        public new class UxmlFactory : UxmlFactory<Card, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _titleKey = new UxmlStringAttributeDescription { name = "title-key" };
            private readonly UxmlStringAttributeDescription _title = new UxmlStringAttributeDescription { name = "title" };
            private readonly UxmlStringAttributeDescription _descriptionKey = new UxmlStringAttributeDescription { name = "description-key" };
            private readonly UxmlStringAttributeDescription _captionKey = new UxmlStringAttributeDescription { name = "caption-key" };
            private readonly UxmlStringAttributeDescription _badgeKey = new UxmlStringAttributeDescription { name = "badge-key" };
            private readonly UxmlStringAttributeDescription _badgeVariant = new UxmlStringAttributeDescription { name = "badge-variant", defaultValue = Badge.NeutralVariant };
            private readonly UxmlBoolAttributeDescription _active = new UxmlBoolAttributeDescription { name = "active" };
            private readonly UxmlBoolAttributeDescription _dimmed = new UxmlBoolAttributeDescription { name = "dimmed" };
            private readonly UxmlIntAttributeDescription _spacing = new UxmlIntAttributeDescription { name = "spacing", defaultValue = DefaultSpacing };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                Card card = (Card)element;
                card.TitleKey = _titleKey.GetValueFromBag(attributes, context);
                card.Title = _title.GetValueFromBag(attributes, context);
                card.DescriptionKey = _descriptionKey.GetValueFromBag(attributes, context);
                card.CaptionKey = _captionKey.GetValueFromBag(attributes, context);
                card.BadgeKey = _badgeKey.GetValueFromBag(attributes, context);
                card.BadgeVariant = _badgeVariant.GetValueFromBag(attributes, context);
                card.Active = _active.GetValueFromBag(attributes, context);
                card.Dimmed = _dimmed.GetValueFromBag(attributes, context);
                card.Spacing = _spacing.GetValueFromBag(attributes, context);
            }
        }

        private readonly VisualElement _header = new VisualElement();
        private readonly Label _title = new Label();
        private readonly VisualElement _spacer = new VisualElement();
        private readonly Label _caption = new Label();
        private readonly Badge _badge = new Badge();
        private readonly Label _description = new Label();
        private readonly VisualElement _body = new VisualElement();
        private int _spacing = DefaultSpacing;

        public Card()
        {
            AddToClassList(ClassName);
            _header.AddToClassList("jtl-card__header");
            _title.AddToClassList("jtl-card__title");
            _spacer.AddToClassList("jtl-card__spacer");
            _caption.AddToClassList("jtl-card__caption");
            _badge.AddToClassList("jtl-card__badge");
            _description.AddToClassList("jtl-card__description");
            _body.AddToClassList("jtl-card__body");
            _body.AddToClassList(StackPrefix + DefaultSpacing);
            _caption.style.display = DisplayStyle.None;
            _badge.style.display = DisplayStyle.None;
            _description.style.display = DisplayStyle.None;
            _header.style.marginBottom = HeaderGap;
            _description.style.marginBottom = HeaderGap;
            _header.Add(_title);
            _header.Add(_spacer);
            _header.Add(_caption);
            _header.Add(_badge);
            hierarchy.Add(_header);
            hierarchy.Add(_description);
            hierarchy.Add(_body);
        }

        public override VisualElement contentContainer => _body;

        public VisualElement Header => _header;

        public string TitleKey { get; set; }

        public string DescriptionKey { get; set; }

        public string CaptionKey { get; set; }

        public string BadgeKey { get; set; }

        public string Title
        {
            get => _title.text;
            set => _title.text = value;
        }

        public string BadgeVariant
        {
            get => _badge.Variant;
            set => _badge.Variant = value;
        }

        public bool Active
        {
            get => ClassListContains(ActiveClass);
            set => EnableInClassList(ActiveClass, value);
        }

        public bool Dimmed
        {
            get => ClassListContains(DimmedClass);
            set => EnableInClassList(DimmedClass, value);
        }

        public int Spacing
        {
            get => _spacing;
            set
            {
                _body.RemoveFromClassList(StackPrefix + _spacing);
                _spacing = value;
                _body.AddToClassList(StackPrefix + _spacing);
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(TitleKey) == false)
            {
                _title.text = localization.Get(TitleKey);
            }

            ApplyOptionalText(_description, DescriptionKey, localization);
            ApplyOptionalText(_caption, CaptionKey, localization);

            if (string.IsNullOrEmpty(BadgeKey))
            {
                _badge.style.display = DisplayStyle.None;
            }
            else
            {
                _badge.Text = localization.Get(BadgeKey);
                _badge.style.display = DisplayStyle.Flex;
            }

            bool hasHeader = string.IsNullOrEmpty(_title.text) == false
                || string.IsNullOrEmpty(CaptionKey) == false
                || string.IsNullOrEmpty(BadgeKey) == false
                || _header.childCount > 4;
            _header.style.display = hasHeader ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ApplyOptionalText(Label label, string key, ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(key))
            {
                label.style.display = DisplayStyle.None;
                return;
            }

            label.text = localization.Get(key);
            label.style.display = DisplayStyle.Flex;
        }
    }
}
