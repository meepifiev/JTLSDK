namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class LeaderboardsSection : ToolkitSection
    {
        private const string StatusTime = "14:08";

        public LeaderboardsSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Leaderboards;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Success, "leaderboards.status", StatusTime);

        protected override string TemplateName => "LeaderboardsSection";
    }
}
