namespace JTLStudio.SDK.Editor.Toolkit
{
    public readonly struct ToolkitStatus
    {
        public readonly StatusKind Kind;
        public readonly string MessageKey;
        public readonly string Time;

        public ToolkitStatus(StatusKind kind, string messageKey, string time)
        {
            Kind = kind;
            MessageKey = messageKey;
            Time = time;
        }
    }
}
