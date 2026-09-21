using System;
using System.Collections.Generic;
using System.Reflection;
using JTLStudio.SDK.Editor.Toolkit.Components;
using JTLStudio.SDK.Editor.Toolkit.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class ProviderRow : VisualElement
    {
        private const int DropdownWidth = 240;
        private const int ChevronSize = 20;

        private readonly ToolkitContext _context;
        private readonly SdkConfiguration _configuration;
        private readonly ModuleSlot _slot;
        private readonly SerializedObject _serialized;
        private readonly Action _onChanged;

        public ProviderRow(ToolkitContext context, SdkConfiguration configuration, ModuleSlot slot, VisualElement leading, Action onChanged)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _slot = slot ?? throw new ArgumentNullException(nameof(slot));
            _onChanged = onChanged;
            _serialized = new SerializedObject(configuration);
            AddToClassList("jtl-column");
            Build(leading);
        }

        private string ExpansionKey => _configuration.GetInstanceID() + ":" + _slot.PropertyName;

        private void Build(VisualElement leading)
        {
            SerializedProperty property = _serialized.FindProperty(_slot.PropertyName);
            Type currentType = CurrentType();
            IReadOnlyList<Type> providers = _context.Providers.ProvidersFor(_slot.ProviderInterface, _configuration.Platform);
            List<SerializedProperty> fields = Fields(property);
            bool expanded = fields.Count > 0 && _context.ExpandedProviders.Contains(ExpansionKey);

            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-module-row");
            row.EnableInClassList("jtl-module-row--dimmed", _context.Providers.IsUnsupported(currentType));

            VisualElement chevronCell = new VisualElement();
            chevronCell.AddToClassList("jtl-module-row__chevron");

            if (fields.Count > 0)
            {
                IconButton chevron = new IconButton(expanded ? "chevron-up" : "chevron-right", ChevronSize);
                chevron.clicked += ToggleExpansion;
                chevronCell.Add(chevron);
            }

            row.Add(chevronCell);
            leading.AddToClassList("jtl-module-row__name");
            row.Add(leading);

            VisualElement providerCell = new VisualElement();
            providerCell.AddToClassList("jtl-module-row__provider");
            Dropdown dropdown = new Dropdown();
            dropdown.style.width = DropdownWidth;
            dropdown.style.minWidth = DropdownWidth;
            List<string> choices = new List<string>();
            int selected = -1;

            for (int index = 0; index < providers.Count; index++)
            {
                choices.Add(_context.Providers.DisplayName(providers[index]));

                if (providers[index] == currentType)
                {
                    selected = index;
                }
            }

            bool placeholder = selected < 0;

            if (placeholder)
            {
                choices.Insert(0, _context.Providers.DisplayName(currentType));
                selected = 0;
            }

            dropdown.choices = choices;
            dropdown.index = selected;
            dropdown.RegisterValueChangedCallback(_ => ChangeProvider(property, providers, placeholder ? dropdown.index - 1 : dropdown.index));
            providerCell.Add(dropdown);
            row.Add(providerCell);
            Add(row);

            if (expanded == false)
            {
                return;
            }

            VisualElement settings = new VisualElement();
            settings.AddToClassList("jtl-module-fields");
            settings.AddToClassList("jtl-provider-fields");

            foreach (SerializedProperty field in fields)
            {
                PropertyField propertyField = new PropertyField(field);
                propertyField.RegisterValueChangeCallback(_ => Save());
                settings.Add(propertyField);
            }

            settings.Bind(_serialized);
            Add(settings);
        }

        private Type CurrentType()
        {
            FieldInfo field = typeof(SdkConfiguration).GetField(_slot.PropertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            object provider = field == null ? null : field.GetValue(_configuration);
            return provider == null ? null : provider.GetType();
        }

        private List<SerializedProperty> Fields(SerializedProperty property)
        {
            List<SerializedProperty> fields = new List<SerializedProperty>();

            if (property == null || property.hasVisibleChildren == false)
            {
                return fields;
            }

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && SerializedProperty.EqualContents(iterator, end) == false)
            {
                enterChildren = false;
                fields.Add(iterator.Copy());
            }

            return fields;
        }

        private void ChangeProvider(SerializedProperty property, IReadOnlyList<Type> providers, int index)
        {
            if (index < 0 || index >= providers.Count || providers[index] == CurrentType())
            {
                return;
            }

            property.managedReferenceValue = Activator.CreateInstance(providers[index]);
            _serialized.ApplyModifiedProperties();
            Save();
            _onChanged?.Invoke();
        }

        private void ToggleExpansion()
        {
            if (_context.ExpandedProviders.Remove(ExpansionKey) == false)
            {
                _context.ExpandedProviders.Add(ExpansionKey);
            }

            _onChanged?.Invoke();
        }

        private void Save()
        {
            _context.Project.Save(_configuration);
            _context.Project.NotifyChanged();
        }
    }
}
