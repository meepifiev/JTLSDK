using System;
using UnityEditor;
using UnityEditor.Build;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class PlayerSettingsPresetService
    {
        private readonly NamedBuildTarget _target = NamedBuildTarget.WebGL;

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
                PlayerSettings.SetManagedStrippingLevel(_target, ToUnity(preset.Stripping));
            }

            if (preset.ApplyRunInBackground)
            {
                PlayerSettings.runInBackground = preset.RunInBackground;
            }

            if (preset.ApplyDebugSymbols)
            {
                PlayerSettings.WebGL.debugSymbolMode = preset.DebugSymbols ? WebGLDebugSymbolMode.External : WebGLDebugSymbolMode.Off;
            }

            if (preset.ApplyMemorySize)
            {
                PlayerSettings.WebGL.memorySize = preset.MemorySizeMegabytes;
            }

            AssetDatabase.SaveAssets();
        }

        public WebCompression CurrentCompression()
        {
            switch (PlayerSettings.WebGL.compressionFormat)
            {
                case WebGLCompressionFormat.Gzip:
                    return WebCompression.Gzip;

                case WebGLCompressionFormat.Brotli:
                    return WebCompression.Brotli;

                default:
                    return WebCompression.Disabled;
            }
        }

        public StrippingLevel CurrentStripping()
        {
            switch (PlayerSettings.GetManagedStrippingLevel(_target))
            {
                case ManagedStrippingLevel.Minimal:
                    return StrippingLevel.Minimal;

                case ManagedStrippingLevel.Low:
                    return StrippingLevel.Low;

                case ManagedStrippingLevel.Medium:
                    return StrippingLevel.Medium;

                case ManagedStrippingLevel.High:
                    return StrippingLevel.High;

                default:
                    return StrippingLevel.Disabled;
            }
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
