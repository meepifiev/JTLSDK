namespace JTLStudio.SDK.Editor.Toolkit.Localization
{
    public readonly struct LocalizedText
    {
        public readonly string English;
        public readonly string Russian;

        public LocalizedText(string english, string russian)
        {
            English = english;
            Russian = russian;
        }
    }
}
