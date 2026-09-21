namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class PurchasesSection : ToolkitSection
    {
        private const string StatusTime = "14:06";

        public PurchasesSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Purchases;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Pending, "purchases.status", StatusTime);

        protected override string TemplateName => "PurchasesSection";
    }
}
