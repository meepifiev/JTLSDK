using System;
using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class NavigationCategory : VisualElement, ILocalizedElement
    {
        private const string ClassName = "jtl-nav-category";
        private const string CollapsedClass = "jtl-nav-category--collapsed";
        private const string ExpandedIcon = "chevron-down";
        private const string CollapsedIcon = "chevron-right";
        private const int ChevronSize = 12;

        public new class UxmlFactory : UxmlFactory<NavigationCategory, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _textKey = new UxmlStringAttributeDescription { name = "text-key" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                ((NavigationCategory)element).TextKey = _textKey.GetValueFromBag(attributes, context);
            }
        }

        private readonly VisualElement _header = new VisualElement();
        private readonly Label _label = new Label();
        private readonly Icon _chevron = new Icon(ExpandedIcon, ChevronSize, "muted");
        private readonly VisualElement _content = new VisualElement();

        public NavigationCategory()
        {
            AddToClassList(ClassName);
            _header.AddToClassList("jtl-nav-category__header");
            _label.AddToClassList("jtl-nav-category__label");
            _label.pickingMode = PickingMode.Ignore;
            _chevron.AddToClassList("jtl-nav-category__chevron");
            _chevron.pickingMode = PickingMode.Ignore;
            _content.AddToClassList("jtl-nav-category__content");
            _header.Add(_label);
            _header.Add(_chevron);
            _header.focusable = true;
            _header.AddManipulator(new Clickable(Toggle));
            hierarchy.Add(_header);
            hierarchy.Add(_content);
        }

        public event Action<NavigationCategory> Toggled;

        public override VisualElement contentContainer => _content;

        public string TextKey { get; set; }

        public bool Collapsed
        {
            get => ClassListContains(CollapsedClass);
            set
            {
                EnableInClassList(CollapsedClass, value);
                _chevron.IconName = value ? CollapsedIcon : ExpandedIcon;
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(TextKey) == false)
            {
                _label.text = localization.Get(TextKey);
            }
        }

        private void Toggle()
        {
            Collapsed = Collapsed == false;
            Toggled?.Invoke(this);
        }
    }
}
