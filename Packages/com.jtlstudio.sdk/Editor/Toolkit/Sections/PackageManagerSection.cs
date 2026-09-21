using JTLStudio.SDK.Editor.Toolkit.Components;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class PackageManagerSection : ToolkitSection
    {
        private const string StatusTime = "14:02";

        public PackageManagerSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.PackageManager;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Warning, "package.status", StatusTime);

        protected override string TemplateName => "PackageManagerSection";

        protected override void OnRendered()
        {
            Require<ToolkitButton>("template-update").SetEnabled(false);
        }
    }
}
