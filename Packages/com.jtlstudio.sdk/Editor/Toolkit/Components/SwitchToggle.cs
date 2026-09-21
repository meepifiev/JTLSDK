using System;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class SwitchToggle : VisualElement
    {
        private const string ClassName = "jtl-switch";
        private const string OnClass = "jtl-switch--on";

        public new class UxmlFactory : UxmlFactory<SwitchToggle, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlBoolAttributeDescription _value = new UxmlBoolAttributeDescription { name = "value" };
            private readonly UxmlBoolAttributeDescription _disabled = new UxmlBoolAttributeDescription { name = "disabled" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                SwitchToggle toggle = (SwitchToggle)element;
                toggle.Value = _value.GetValueFromBag(attributes, context);
                toggle.SetEnabled(_disabled.GetValueFromBag(attributes, context) == false);
            }
        }

        private readonly VisualElement _knob = new VisualElement();
        private bool _value;

        public SwitchToggle()
        {
            AddToClassList(ClassName);
            _knob.AddToClassList("jtl-switch__knob");
            _knob.pickingMode = PickingMode.Ignore;
            Add(_knob);
            focusable = true;
            this.AddManipulator(new Clickable(OnClicked));
        }

        public SwitchToggle(bool value) : this()
        {
            Value = value;
        }

        public event Action<bool> ValueChanged;

        public bool Value
        {
            get => _value;
            set
            {
                _value = value;
                EnableInClassList(OnClass, _value);
            }
        }

        private void OnClicked()
        {
            Value = _value == false;
            ValueChanged?.Invoke(_value);
        }
    }
}
