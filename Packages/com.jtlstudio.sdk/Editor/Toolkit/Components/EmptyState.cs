using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class EmptyState : VisualElement, ILocalizedElement
    {
        public const int DefaultMinHeight = 180;
        private const int IconSize = 20;
        private const string ClassName = "jtl-empty";

        public new class UxmlFactory : UxmlFactory<EmptyState, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _icon = new UxmlStringAttributeDescription { name = "icon" };
            private readonly UxmlStringAttributeDescription _titleKey = new UxmlStringAttributeDescription { name = "title-key" };
            private readonly UxmlStringAttributeDescription _descriptionKey = new UxmlStringAttributeDescription { name = "description-key" };
            private readonly UxmlIntAttributeDescription _minHeight = new UxmlIntAttributeDescription { name = "min-height", defaultValue = DefaultMinHeight };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                EmptyState state = (EmptyState)element;
                state.IconName = _icon.GetValueFromBag(attributes, context);
                state.TitleKey = _titleKey.GetValueFromBag(attributes, context);
                state.DescriptionKey = _descriptionKey.GetValueFromBag(attributes, context);
                state.style.minHeight = _minHeight.GetValueFromBag(attributes, context);
            }
        }

        private readonly Icon _icon = new Icon();
        private readonly Label _title = new Label();
        private readonly Label _description = new Label();
        private readonly VisualElement _actions = new VisualElement();

        public EmptyState()
        {
            AddToClassList(ClassName);
            _icon.AddToClassList("jtl-empty__icon");
            _icon.Size = IconSize;
            _icon.Tone = "muted";
            _title.AddToClassList("jtl-empty__title");
            _description.AddToClassList("jtl-empty__description");
            _actions.AddToClassList("jtl-empty__actions");
            _actions.AddToClassList("jtl-hstack-8");
            hierarchy.Add(_icon);
            hierarchy.Add(_title);
            hierarchy.Add(_description);
            hierarchy.Add(_actions);
        }

        public override VisualElement contentContainer => _actions;

        public string TitleKey { get; set; }

        public string DescriptionKey { get; set; }

        public string IconName
        {
            get => _icon.IconName;
            set => _icon.IconName = value;
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(TitleKey) == false)
            {
                _title.text = localization.Get(TitleKey);
            }

            if (string.IsNullOrEmpty(DescriptionKey) == false)
            {
                _description.text = localization.Get(DescriptionKey);
            }
        }
    }
}
