using System;
using System.Collections.Generic;
using System.Globalization;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit
{
    public abstract class ToolkitSection
    {
        private const string ClassName = "jtl-section";
        private const string TemplateFolder = "Sections/";
        private const string TemplateExtension = ".uxml";

        private readonly ToolkitContext _context;
        private readonly VisualElement _root = new VisualElement();
        private readonly LayoutGaps _gaps = new LayoutGaps();
        private readonly MonospaceFont _monospace = new MonospaceFont();
        private VisualTreeAsset _template;

        protected ToolkitSection(ToolkitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _root.AddToClassList(ClassName);
        }

        public abstract ToolkitSectionId Id { get; }

        public abstract ToolkitStatus Status { get; }

        public virtual ToolkitSectionId NavigationId => Id;

        public VisualElement Root => _root;

        protected ToolkitContext Context => _context;

        protected abstract string TemplateName { get; }

        public void Render()
        {
            _root.Clear();

            if (_template == null)
            {
                _template = _context.Assets.LoadTemplate(TemplateFolder + TemplateName + TemplateExtension);
            }

            _template.CloneTree(_root);
            OnRendered();
            Localize(_root);
            _gaps.Apply(_root);
            _monospace.Apply(_root);
        }

        protected virtual void OnRendered()
        {
        }

        protected void Localize(VisualElement element)
        {
            List<VisualElement> elements = element.Query<VisualElement>().ToList();

            foreach (VisualElement candidate in elements)
            {
                if (candidate is ILocalizedElement localized)
                {
                    localized.ApplyLocalization(_context.Localization);
                }
            }
        }

        protected T Require<T>(string elementName) where T : VisualElement
        {
            T element = _root.Q<T>(elementName);

            if (element == null)
            {
                throw new InvalidOperationException(nameof(elementName));
            }

            return element;
        }

        protected VisualElement Row(int gap)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-row");
            row.AddToClassList("jtl-hstack-" + gap.ToString(CultureInfo.InvariantCulture));
            return row;
        }

        protected VisualElement Column(int gap)
        {
            VisualElement column = new VisualElement();
            column.AddToClassList("jtl-column");
            column.AddToClassList("jtl-vstack-" + gap.ToString(CultureInfo.InvariantCulture));
            return column;
        }

        protected VisualElement Spacer()
        {
            VisualElement spacer = new VisualElement();
            spacer.AddToClassList("jtl-spacer");
            return spacer;
        }

        protected Label TextLabel(string text, params string[] classNames)
        {
            Label label = new Label(text);

            foreach (string className in classNames)
            {
                label.AddToClassList(className);
            }

            return label;
        }

        protected LocalizedLabel Localized(string key, params string[] classNames)
        {
            LocalizedLabel label = new LocalizedLabel(key);

            foreach (string className in classNames)
            {
                label.AddToClassList(className);
            }

            return label;
        }

        protected ToolkitButton Button(string textKey, string variant, string iconName, Action onClick)
        {
            ToolkitButton button = new ToolkitButton(textKey, variant);

            if (string.IsNullOrEmpty(iconName) == false)
            {
                button.IconName = iconName;
            }

            button.clicked += onClick;
            return button;
        }

        protected void NavigateTo(ToolkitSectionId sectionId)
        {
            _context.Navigate(sectionId);
        }

        protected void ShowStatus(ToolkitStatus status)
        {
            _context.ShowStatus(status);
        }
    }
}
