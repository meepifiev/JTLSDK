using JTLStudio.SDK.Editor.Toolkit.Components;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class TemplateSection : ToolkitSection
    {
        private const string StatusTime = "14:05";

        public TemplateSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Template;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "TemplateSection";

        protected override void OnRendered()
        {
            SegmentedControl modes = new SegmentedControl();
            modes.AddToClassList("jtl-segmented--medium");
            modes.ChoicesKey = "template.previewModes";
            Require<Card>("preview-card").Header.Add(modes);
        }
    }
}
