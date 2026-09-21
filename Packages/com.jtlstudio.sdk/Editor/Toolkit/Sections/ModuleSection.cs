using System;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class ModuleSection : ToolkitSection
    {
        private readonly ToolkitSectionId _id;
        private readonly string _titleKey;
        private readonly string _propertyName;

        public ModuleSection(ToolkitContext context, ToolkitSectionId id, string titleKey, string propertyName) : base(context)
        {
            _id = id;
            _titleKey = titleKey ?? throw new ArgumentNullException(nameof(titleKey));
            _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        }

        public override ToolkitSectionId Id => _id;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "ModuleSection";

        protected override void OnRendered()
        {
            Require<SectionHeader>("module-header").TitleKey = _titleKey;
            Require<VisualElement>("module-body").Add(ProvidersCard(_propertyName));
        }
    }
}
