using System;
using System.Globalization;
using UnityEngine;

namespace JTLStudio.SDK.Editor.Configuration
{
    [Serializable]
    public class TemplateBackground
    {
        [SerializeField] private BackgroundKind _kind = BackgroundKind.Color;
        [SerializeField] private Color _color = new Color(0.102f, 0.102f, 0.102f);
        [SerializeField] private Color _gradientFrom = new Color(0.169f, 0.106f, 0.42f);
        [SerializeField] private Color _gradientTo = new Color(0.051f, 0.039f, 0.122f);
        [SerializeField] private bool _radial;
        [SerializeField] private int _angle = 140;
        [SerializeField] private Texture2D _image;

        public BackgroundKind Kind { get => _kind; set => _kind = value; }
        public Color Color { get => _color; set => _color = value; }
        public Color GradientFrom { get => _gradientFrom; set => _gradientFrom = value; }
        public Color GradientTo { get => _gradientTo; set => _gradientTo = value; }
        public bool Radial { get => _radial; set => _radial = value; }
        public int Angle { get => _angle; set => _angle = value; }
        public Texture2D Image { get => _image; set => _image = value; }

        public string ToCss(string imageFileName)
        {
            switch (_kind)
            {
                case BackgroundKind.Gradient:
                    return _radial
                        ? "radial-gradient(circle, " + Hex(_gradientFrom) + ", " + Hex(_gradientTo) + ")"
                        : "linear-gradient(" + _angle.ToString(CultureInfo.InvariantCulture) + "deg, " + Hex(_gradientFrom) + ", " + Hex(_gradientTo) + ")";

                case BackgroundKind.Image:
                    return _image == null ? Hex(_color) : "url(TemplateData/" + imageFileName + ") center / cover no-repeat, " + Hex(_color);

                default:
                    return Hex(_color);
            }
        }

        private string Hex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }
    }
}
