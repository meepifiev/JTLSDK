namespace JTLStudio.SDK.Editor.Build
{
    public class BuildCheck
    {
        public BuildCheck(string key, bool passed, params object[] arguments)
        {
            Key = key;
            Passed = passed;
            Arguments = arguments;
        }

        public string Key { get; }
        public bool Passed { get; }
        public object[] Arguments { get; }
    }
}
