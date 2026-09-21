using JTLStudio.SDK.Editor.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace JTLStudio.SDK.Tests.Build
{
    public class TemplateSettingsTests
    {
        [Test]
        public void ColorBackgroundIsHex()
        {
            TemplateBackground background = new TemplateBackground { Kind = BackgroundKind.Color, Color = Color.red };

            Assert.AreEqual("#FF0000", background.ToCss("background.png"));
        }

        [Test]
        public void LinearGradientUsesAngle()
        {
            TemplateBackground background = new TemplateBackground { Kind = BackgroundKind.Gradient, GradientFrom = Color.black, GradientTo = Color.white, Angle = 90 };

            Assert.AreEqual("linear-gradient(90deg, #000000, #FFFFFF)", background.ToCss("background.png"));
        }

        [Test]
        public void RadialGradientIgnoresAngle()
        {
            TemplateBackground background = new TemplateBackground { Kind = BackgroundKind.Gradient, GradientFrom = Color.black, GradientTo = Color.white, Radial = true };

            Assert.AreEqual("radial-gradient(circle, #000000, #FFFFFF)", background.ToCss("background.png"));
        }

        [Test]
        public void ImageWithoutTextureFallsBackToColor()
        {
            TemplateBackground background = new TemplateBackground { Kind = BackgroundKind.Image, Color = Color.blue };

            Assert.AreEqual("#0000FF", background.ToCss("background.png"));
        }

        [Test]
        public void ProgressAndBuildNumberAreClamped()
        {
            JTLSDKEditorSettings settings = JTLSDKEditorSettings.instance;
            int width = settings.ProgressWidthPercent;
            int number = settings.BuildNumber;

            settings.ProgressWidthPercent = 500;
            settings.BuildNumber = -3;

            Assert.AreEqual(100, settings.ProgressWidthPercent);
            Assert.AreEqual(0, settings.BuildNumber);

            settings.ProgressWidthPercent = width;
            settings.BuildNumber = number;
        }
    }
}
