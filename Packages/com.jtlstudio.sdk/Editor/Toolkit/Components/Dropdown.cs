using System.Collections.Generic;
using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class Dropdown : DropdownField, ILocalizedElement
    {
        private const string ClassName = "jtl-dropdown";
        private const string ErrorClass = "jtl-dropdown--error";
        private const string DisabledClass = "jtl-dropdown--disabled";
        private const int ChevronSize = 16;

        public new class UxmlFactory : UxmlFactory<Dropdown, UxmlTraits>
        {
        }

        public new class UxmlTraits : DropdownField.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _choicesKey = new UxmlStringAttributeDescription { name = "choices-key" };
            private readonly UxmlIntAttributeDescription _selected = new UxmlIntAttributeDescription { name = "selected" };
            private readonly UxmlIntAttributeDescription _width = new UxmlIntAttributeDescription { name = "width" };
            private readonly UxmlBoolAttributeDescription _error = new UxmlBoolAttributeDescription { name = "error" };
            private readonly UxmlBoolAttributeDescription _disabled = new UxmlBoolAttributeDescription { name = "disabled" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                Dropdown dropdown = (Dropdown)element;
                dropdown.ChoicesKey = _choicesKey.GetValueFromBag(attributes, context);
                dropdown.Selected = _selected.GetValueFromBag(attributes, context);
                dropdown.Error = _error.GetValueFromBag(attributes, context);
                dropdown.Disabled = _disabled.GetValueFromBag(attributes, context);
                int width = _width.GetValueFromBag(attributes, context);

                if (width > 0)
                {
                    dropdown.style.width = width;
                    dropdown.style.minWidth = width;
                }
            }
        }

        private readonly Icon _chevron = new Icon("chevron-down", ChevronSize, "muted");

        public Dropdown()
        {
            AddToClassList(ClassName);
            label = null;
            VisualElement input = this.Q(className: "unity-base-popup-field__input");
            VisualElement arrow = this.Q(className: "unity-base-popup-field__arrow");

            if (arrow != null)
            {
                arrow.style.display = DisplayStyle.None;
            }

            if (input != null)
            {
                _chevron.AddToClassList("jtl-dropdown__chevron");
                input.Add(_chevron);
            }
        }

        public string ChoicesKey { get; set; }

        public int Selected { get; set; }

        public bool Error
        {
            get => ClassListContains(ErrorClass);
            set => EnableInClassList(ErrorClass, value);
        }

        public bool Disabled
        {
            get => ClassListContains(DisabledClass);
            set
            {
                EnableInClassList(DisabledClass, value);
                SetEnabled(value == false);
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(ChoicesKey))
            {
                return;
            }

            choices = new List<string>(localization.GetList(ChoicesKey));
            index = Selected < choices.Count ? Selected : 0;
        }
    }
}
