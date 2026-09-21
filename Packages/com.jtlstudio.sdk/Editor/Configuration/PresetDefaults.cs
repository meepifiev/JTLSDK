namespace JTLStudio.SDK.Editor.Configuration
{
    public class PresetDefaults
    {
        public void Apply(PlayerSettingsPreset preset, PlatformId platform)
        {
            preset.Template = PlayerSettingsPreset.DefaultTemplate;
            preset.DecompressionFallback = false;
            preset.DataCaching = true;
            preset.RunInBackground = true;
            preset.MemorySizeMegabytes = 512;
            preset.DebugSymbols = false;

            switch (platform)
            {
                case PlatformId.YouTubePlayables:
                    preset.Compression = WebCompression.Disabled;
                    preset.Stripping = StrippingLevel.High;
                    break;

                default:
                    preset.Compression = WebCompression.Brotli;
                    preset.Stripping = StrippingLevel.Medium;
                    break;
            }
        }
    }
}
