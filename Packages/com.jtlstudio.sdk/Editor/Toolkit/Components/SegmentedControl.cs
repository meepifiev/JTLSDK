using System;
using System.Collections.Generic;
using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class SegmentedControl : VisualElement, ILocalizedElement
    {
        private const char Separator = '|';
        private const string ClassName = "jtl-segmented";
        private const string OptionClass = "jtl-segmented__option";
        private const string SelectedClass = "jtl-segmented__option--selected";
        private const string FirstClass = "jtl-segmented__option--first";

        public new class UxmlFactory : UxmlFactory<SegmentedControl, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _choices = new UxmlStringAttributeDescription { name = "choices" };
            private readonly UxmlStringAttributeDescription _choicesKey = new UxmlStringAttributeDescription { name = "choices-key" };
            private readonly UxmlIntAttributeDescription _index = new UxmlIntAttributeDescription { name = "index" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                SegmentedControl control = (SegmentedControl)element;
                control.ChoicesKey = _choicesKey.GetValueFromBag(attributes, context);
                string choices = _choices.GetValueFromBag(attributes, context);

                if (string.IsNullOrEmpty(choices) == false)
                {
                    control.SetChoices(choices.Split(Separator));
                }

                control.Index = _index.GetValueFromBag(attributes, context);
            }
        }

        private readonly List<VisualElement> _options = new List<VisualElement>();
        private int _index;

        public SegmentedControl()
        {
            AddToClassList(ClassName);
        }

        public event Action<int> IndexChanged;

        public string ChoicesKey { get; set; }

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                ApplySelection();
            }
        }

        public void SetChoices(IReadOnlyList<string> choices)
        {
            if (choices == null)
            {
                throw new ArgumentNullException(nameof(choices));
            }

            Clear();
            _options.Clear();

            for (int optionIndex = 0; optionIndex < choices.Count; optionIndex++)
            {
                Label option = new Label(choices[optionIndex]);
                option.AddToClassList(OptionClass);
                option.EnableInClassList(FirstClass, optionIndex == 0);
                option.focusable = true;
                int capturedIndex = optionIndex;
                option.AddManipulator(new Clickable(() => OnOptionClicked(capturedIndex)));
                _options.Add(option);
                Add(option);
            }

            ApplySelection();
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(ChoicesKey) == false)
            {
                SetChoices(localization.GetList(ChoicesKey));
            }
        }

        private void ApplySelection()
        {
            for (int optionIndex = 0; optionIndex < _options.Count; optionIndex++)
            {
                _options[optionIndex].EnableInClassList(SelectedClass, optionIndex == _index);
            }
        }

        private void OnOptionClicked(int optionIndex)
        {
            if (optionIndex == _index)
            {
                return;
            }

            Index = optionIndex;
            IndexChanged?.Invoke(_index);
        }
    }
}
