using System;
using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class NavigationItem : VisualElement, ILocalizedElement
    {
        private const string ClassName = "jtl-nav-item";
        private const string SelectedClass = "jtl-nav-item--selected";

        public new class UxmlFactory : UxmlFactory<NavigationItem, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _icon = new UxmlStringAttributeDescription { name = "icon" };
            private readonly UxmlStringAttributeDescription _textKey = new UxmlStringAttributeDescription { name = "text-key" };
            private readonly UxmlEnumAttributeDescription<ToolkitSectionId> _section = new UxmlEnumAttributeDescription<ToolkitSectionId> { name = "section" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                NavigationItem item = (NavigationItem)element;
                item.IconName = _icon.GetValueFromBag(attributes, context);
                item.TextKey = _textKey.GetValueFromBag(attributes, context);
                item.Section = _section.GetValueFromBag(attributes, context);
            }
        }

        private const int IconSize = 15;

        private readonly VisualElement _tile = new VisualElement();
        private readonly Icon _icon = new Icon { Size = IconSize };
        private readonly Label _label = new Label();

        public NavigationItem()
        {
            AddToClassList(ClassName);
            _tile.AddToClassList("jtl-nav-item__tile");
            _tile.pickingMode = PickingMode.Ignore;
            _icon.AddToClassList("jtl-nav-item__icon");
            _icon.pickingMode = PickingMode.Ignore;
            _label.AddToClassList("jtl-nav-item__label");
            _label.pickingMode = PickingMode.Ignore;
            _tile.Add(_icon);
            Add(_tile);
            Add(_label);
            focusable = true;
            this.AddManipulator(new Clickable(OnClicked));
        }

        public event Action<NavigationItem> Clicked;

        public ToolkitSectionId Section { get; set; }

        public string TextKey { get; set; }

        public string IconName
        {
            get => _icon.IconName;
            set => _icon.IconName = value;
        }

        public string Text => _label.text;

        public bool Selected
        {
            get => ClassListContains(SelectedClass);
            set => EnableInClassList(SelectedClass, value);
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(TextKey) == false)
            {
                _label.text = localization.Get(TextKey);
                tooltip = _label.text;
            }
        }

        private void OnClicked()
        {
            Clicked?.Invoke(this);
        }
    }
}
