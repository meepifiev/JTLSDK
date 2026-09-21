namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class SimulationSection : ToolkitSection
    {
        private const string StatusTime = "14:09";

        public SimulationSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Simulation;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Pending, "simulation.status", StatusTime);

        protected override string TemplateName => "SimulationSection";
    }
}
