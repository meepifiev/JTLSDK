namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class FlagsSection : ToolkitSection
    {
        private const string StatusTime = "14:08";

        public FlagsSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Flags;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Success, "leaderboards.status", StatusTime);

        protected override string TemplateName => "FlagsSection";
    }
}
