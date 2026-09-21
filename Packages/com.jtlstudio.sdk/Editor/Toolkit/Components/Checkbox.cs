using System;
using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class Checkbox : VisualElement, ILocalizedElement
    {
        private const int MarkSize = 10;
        private const string ClassName = "jtl-checkbox";
        private const string CheckedClass = "jtl-checkbox--checked";

        public new class UxmlFactory : UxmlFactory<Checkbox, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _labelKey = new UxmlStringAttributeDescription { name = "label-key" };
            private readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };
            private readonly UxmlBoolAttributeDescription _value = new UxmlBoolAttributeDescription { name = "value" };
            private readonly UxmlBoolAttributeDescription _disabled = new UxmlBoolAttributeDescription { name = "disabled" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                Checkbox checkbox = (Checkbox)element;
                checkbox.LabelKey = _labelKey.GetValueFromBag(attributes, context);
                checkbox.Label = _label.GetValueFromBag(attributes, context);
                checkbox.Value = _value.GetValueFromBag(attributes, context);
                checkbox.SetEnabled(_disabled.GetValueFromBag(attributes, context) == false);
            }
        }

        private readonly VisualElement _box = new VisualElement();
        private readonly Icon _mark = new Icon("check", MarkSize, "on-accent");
        private readonly Label _label = new Label();
        private bool _value;

        public Checkbox()
        {
            AddToClassList(ClassName);
            _box.AddToClassList("jtl-checkbox__box");
            _box.pickingMode = PickingMode.Ignore;
            _mark.AddToClassList("jtl-checkbox__mark");
            _label.AddToClassList("jtl-checkbox__label");
            _label.pickingMode = PickingMode.Ignore;
            _box.Add(_mark);
            Add(_box);
            Add(_label);
            focusable = true;
            this.AddManipulator(new Clickable(OnClicked));
        }

        public Checkbox(string label, bool value) : this()
        {
            Label = label;
            Value = value;
        }

        public event Action<bool> ValueChanged;

        public string LabelKey { get; set; }

        public string Label
        {
            get => _label.text;
            set
            {
                _label.text = value;
                _label.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        public bool Value
        {
            get => _value;
            set
            {
                _value = value;
                EnableInClassList(CheckedClass, _value);
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(LabelKey) == false)
            {
                Label = localization.Get(LabelKey);
            }
        }

        private void OnClicked()
        {
            Value = _value == false;
            ValueChanged?.Invoke(_value);
        }
    }
}
