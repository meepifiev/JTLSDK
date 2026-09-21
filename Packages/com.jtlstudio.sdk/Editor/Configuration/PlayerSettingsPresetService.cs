using System;
using UnityEditor;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class PlayerSettingsPresetService
    {
        private const BuildTargetGroup TargetGroup = BuildTargetGroup.WebGL;

        public void Apply(PlayerSettingsPreset preset)
        {
            if (preset == null)
            {
                throw new ArgumentNullException(nameof(preset));
            }

            if (preset.ApplyTemplate)
            {
                PlayerSettings.WebGL.template = preset.Template;
            }

            if (preset.ApplyCompression)
            {
                PlayerSettings.WebGL.compressionFormat = ToUnity(preset.Compression);
            }

            if (preset.ApplyDecompressionFallback)
            {
                PlayerSettings.WebGL.decompressionFallback = preset.DecompressionFallback;
            }

            if (preset.ApplyDataCaching)
            {
                PlayerSettings.WebGL.dataCaching = preset.DataCaching;
            }

            if (preset.ApplyStripping)
            {
                PlayerSettings.SetManagedStrippingLevel(TargetGroup, ToUnity(preset.Stripping));
            }

            if (preset.ApplyRunInBackground)
            {
                PlayerSettings.runInBackground = preset.RunInBackground;
            }

            if (preset.ApplyMemorySize)
            {
                PlayerSettings.WebGL.memorySize = preset.MemorySizeMegabytes;
            }

            AssetDatabase.SaveAssets();
        }

        private WebGLCompressionFormat ToUnity(WebCompression compression)
        {
            switch (compression)
            {
                case WebCompression.Disabled:
                    return WebGLCompressionFormat.Disabled;

                case WebCompression.Gzip:
                    return WebGLCompressionFormat.Gzip;

                case WebCompression.Brotli:
                    return WebGLCompressionFormat.Brotli;

                default:
                    throw new ArgumentOutOfRangeException(nameof(compression));
            }
        }

        private ManagedStrippingLevel ToUnity(StrippingLevel stripping)
        {
            switch (stripping)
            {
                case StrippingLevel.Disabled:
                    return ManagedStrippingLevel.Disabled;

                case StrippingLevel.Minimal:
                    return ManagedStrippingLevel.Minimal;

                case StrippingLevel.Low:
                    return ManagedStrippingLevel.Low;

                case StrippingLevel.Medium:
                    return ManagedStrippingLevel.Medium;

                case StrippingLevel.High:
                    return ManagedStrippingLevel.High;

                default:
                    throw new ArgumentOutOfRangeException(nameof(stripping));
            }
        }
    }
}
