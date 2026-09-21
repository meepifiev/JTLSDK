using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class SectionHeader : VisualElement, ILocalizedElement
    {
        private const string ClassName = "jtl-section-header";

        public new class UxmlFactory : UxmlFactory<SectionHeader, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _titleKey = new UxmlStringAttributeDescription { name = "title-key" };
            private readonly UxmlStringAttributeDescription _descriptionKey = new UxmlStringAttributeDescription { name = "description-key" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                SectionHeader header = (SectionHeader)element;
                header.TitleKey = _titleKey.GetValueFromBag(attributes, context);
                header.DescriptionKey = _descriptionKey.GetValueFromBag(attributes, context);
            }
        }

        private readonly VisualElement _text = new VisualElement();
        private readonly Label _title = new Label();
        private readonly Label _description = new Label();
        private readonly VisualElement _actions = new VisualElement();

        public SectionHeader()
        {
            AddToClassList(ClassName);
            _text.AddToClassList("jtl-section-header__text");
            _title.AddToClassList("jtl-section-header__title");
            _description.AddToClassList("jtl-section-header__description");
            _actions.AddToClassList("jtl-section-header__actions");
            _actions.AddToClassList("jtl-hstack-8");
            _text.Add(_title);
            _text.Add(_description);
            hierarchy.Add(_text);
            hierarchy.Add(_actions);
        }

        public override VisualElement contentContainer => _actions;

        public string TitleKey { get; set; }

        public string DescriptionKey { get; set; }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(TitleKey) == false)
            {
                _title.text = localization.Get(TitleKey);
            }

            bool hasDescription = string.IsNullOrEmpty(DescriptionKey) == false;
            _description.style.display = hasDescription ? DisplayStyle.Flex : DisplayStyle.None;
            _description.text = hasDescription ? localization.Get(DescriptionKey) : string.Empty;
        }
    }
}
