using System;
using System.Collections.Generic;
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
