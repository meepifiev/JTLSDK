using System;
using System.Globalization;
using JTLStudio.SDK.Editor.Configuration;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class TemplateSection : ToolkitSection
    {
        private const int LabelWidth = 170;
        private const int PreviewWidth = 400;
        private const int DesktopPreviewHeight = 250;
        private const int MobilePreviewHeight = 520;

        private readonly TemplateService _template = new TemplateService();
        private bool _mobilePreview;
        private float _previewProgress = 0.55f;

        public TemplateSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Template;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "TemplateSection";

        private JTLSDKEditorSettings Settings => JTLSDKEditorSettings.instance;

        protected override void OnRendered()
        {
            SectionHeader header = Require<SectionHeader>("template-header");

            if (_template.IsInstalled)
            {
                header.Add(new Badge("template.installed", Badge.SuccessVariant));
                header.Add(Button("template.reinstall", ToolkitButton.GhostVariant, "refresh", Install));
            }
            else
            {
                header.Add(Button("template.install", ToolkitButton.PrimaryVariant, "plus", Install));
            }

            VisualElement body = Require<VisualElement>("template-body");
            VisualElement left = Column(12);
            left.AddToClassList("jtl-basis");
            left.Add(CreateLogoCard());
            left.Add(CreateLoaderCard());
            left.Add(CreateBackgroundCard("template.pageBackground", Settings.PageBackground));
            left.Add(CreateCanvasCard());
            body.Add(left);
            body.Add(CreatePreviewCard());
        }

        private VisualElement CreateLogoCard()
        {
            Card card = new Card { TitleKey = "template.logo" };
            card.Add(Field("template.logoFile", TextureInput(Settings.Logo, texture => Settings.Logo = texture)));
            card.Add(Field("template.logoSize", IntegerInput(Settings.LogoSize, "unit.px", value => Settings.LogoSize = value)));
            return card;
        }

        private VisualElement CreateLoaderCard()
        {
            Card card = new Card { TitleKey = "template.loadingScreen" };
            AddBackgroundFields(card, Settings.LoaderBackground);
            card.Add(Field("template.progressFill", ColorInput(Settings.ProgressFill, color => Settings.ProgressFill = color)));
            card.Add(Field("template.progressTrack", ColorInput(Settings.ProgressTrack, color => Settings.ProgressTrack = color)));

            VisualElement size = Row(8);
            size.Add(IntegerInput(Settings.ProgressWidthPercent, "unit.percent", value => Settings.ProgressWidthPercent = value));
            size.Add(IntegerInput(Settings.ProgressHeight, "unit.px", value => Settings.ProgressHeight = value));
            size.Add(IntegerInput(Settings.ProgressRadius, "unit.px", value => Settings.ProgressRadius = value));
            card.Add(Field("template.progressSize", size));

            RadioGroup position = new RadioGroup();
            position.SetChoices(Context.Localization.GetList("template.positions"));
            position.Index = Settings.ProgressAtBottom ? 1 : 0;
            position.IndexChanged += index => Change(() => Settings.ProgressAtBottom = index == 1);
            card.Add(Field("template.progressPosition", position));
            card.Add(Field("template.loadingText", TextInput(Settings.LoadingText, value => Settings.LoadingText = value)));
            return card;
        }

        private VisualElement CreateBackgroundCard(string titleKey, TemplateBackground background)
        {
            Card card = new Card { TitleKey = titleKey };
            AddBackgroundFields(card, background);
            return card;
        }

        private void AddBackgroundFields(Card card, TemplateBackground background)
        {
            RadioGroup kind = new RadioGroup();
            kind.SetChoices(Context.Localization.GetList("template.backgroundKinds"));
            kind.Index = (int)background.Kind;
            kind.IndexChanged += index => Change(() => background.Kind = (BackgroundKind)index);
            card.Add(Field("template.background", kind));

            switch (background.Kind)
            {
                case BackgroundKind.Gradient:
                    card.Add(Field("template.gradientFrom", ColorInput(background.GradientFrom, color => background.GradientFrom = color)));
                    card.Add(Field("template.gradientTo", ColorInput(background.GradientTo, color => background.GradientTo = color)));
                    SwitchToggle radial = new SwitchToggle(background.Radial);
                    radial.ValueChanged += value => Change(() => background.Radial = value);
                    card.Add(Field("template.radial", radial));

                    if (background.Radial == false)
                    {
                        card.Add(Field("template.angle", IntegerInput(background.Angle, "unit.deg", value => background.Angle = value)));
                    }

                    break;

                case BackgroundKind.Image:
                    card.Add(Field("template.image", TextureInput(background.Image, texture => background.Image = texture)));
                    card.Add(Field("template.backgroundColor", ColorInput(background.Color, color => background.Color = color)));
                    break;

                default:
                    card.Add(Field("template.backgroundColor", ColorInput(background.Color, color => background.Color = color)));
                    break;
            }
        }

        private VisualElement CreateCanvasCard()
        {
            Card card = new Card { TitleKey = "template.canvas" };
            SwitchToggle fixedAspect = new SwitchToggle(Settings.FixedAspect);
            fixedAspect.ValueChanged += value => Change(() => Settings.FixedAspect = value);
            card.Add(Field("template.fixedAspect", fixedAspect));

            if (Settings.FixedAspect)
            {
                TextField ratio = TextInput(Settings.AspectRatio, value => Settings.AspectRatio = value);
                ratio.style.width = 110;
                ratio.style.flexGrow = 0;
                card.Add(Field("template.aspectRatio", ratio));
                SwitchToggle mobile = new SwitchToggle(Settings.FreeAspectOnMobile);
                mobile.ValueChanged += value => Change(() => Settings.FreeAspectOnMobile = value);
                card.Add(Field("template.freeOnMobile", mobile));
            }

            card.Add(Field("template.pixelRatioDesktop", PixelRatioInput(Settings.DesktopPixelRatioMode, Settings.DesktopPixelRatio, mode => Settings.DesktopPixelRatioMode = mode, value => Settings.DesktopPixelRatio = value)));
            card.Add(Field("template.pixelRatioMobile", PixelRatioInput(Settings.MobilePixelRatioMode, Settings.MobilePixelRatio, mode => Settings.MobilePixelRatioMode = mode, value => Settings.MobilePixelRatio = value)));
            SwitchToggle fullscreen = new SwitchToggle(Settings.FullscreenButton);
            fullscreen.ValueChanged += value => Change(() => Settings.FullscreenButton = value);
            card.Add(Field("template.fullscreenButton", fullscreen));
            return card;
        }

        private VisualElement PixelRatioInput(PixelRatioMode mode, float value, Action<PixelRatioMode> assignMode, Action<float> assignValue)
        {
            VisualElement row = Row(8);
            Dropdown modes = new Dropdown();
            modes.style.width = 170;
            modes.choices = new System.Collections.Generic.List<string>(Context.Localization.GetList("template.pixelRatioModes"));
            modes.index = (int)mode;
            modes.RegisterValueChangedCallback(_ => Change(() => assignMode((PixelRatioMode)modes.index)));
            row.Add(modes);

            if (mode != PixelRatioMode.Auto)
            {
                NumberFieldWithUnit number = new NumberFieldWithUnit { Width = 80 };
                number.Value = value.ToString("0.##", CultureInfo.InvariantCulture);
                number.Input.RegisterCallback<FocusOutEvent>(focusEvent =>
                {
                    if (float.TryParse(number.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
                    {
                        Change(() => assignValue(parsed));
                    }
                });
                row.Add(number);
            }

            return row;
        }

        private VisualElement CreatePreviewCard()
        {
            Card card = new Card { TitleKey = "template.preview", Spacing = 10 };
            card.style.width = PreviewWidth + 34;
            card.style.flexShrink = 0;

            SegmentedControl device = new SegmentedControl();
            device.SetChoices(Context.Localization.GetList("template.previewDevices"));
            device.Index = _mobilePreview ? 1 : 0;
            device.IndexChanged += index =>
            {
                _mobilePreview = index == 1;
                Render();
            };
            card.Header.Add(device);

            VisualElement frame = new VisualElement();
            frame.AddToClassList("jtl-template-preview");
            int width = _mobilePreview ? 240 : PreviewWidth;
            frame.style.width = width;
            frame.style.height = _mobilePreview ? MobilePreviewHeight : DesktopPreviewHeight;
            frame.style.alignSelf = Align.Center;
            frame.style.backgroundColor = PreviewColor(Settings.LoaderBackground);

            if (Settings.LoaderBackground.Kind == BackgroundKind.Image && Settings.LoaderBackground.Image != null)
            {
                frame.style.backgroundImage = Settings.LoaderBackground.Image;
                SetScaleMode(frame, ScaleMode.ScaleAndCrop);
            }

            VisualElement logo = new VisualElement();
            float logoWidth = Mathf.Min(Settings.LogoSize, width * 0.6f);
            logo.style.width = logoWidth;
            logo.style.height = Settings.Logo == null ? logoWidth * 0.5f : logoWidth * Settings.Logo.height / Mathf.Max(1f, Settings.Logo.width);
            SetScaleMode(logo, ScaleMode.ScaleToFit);

            if (Settings.Logo != null)
            {
                logo.style.backgroundImage = Settings.Logo;
            }
            else
            {
                logo.AddToClassList("jtl-template-preview__logo-placeholder");
            }

            frame.Add(logo);

            VisualElement track = new VisualElement();
            track.style.width = Length.Percent(Settings.ProgressWidthPercent);
            track.style.height = Settings.ProgressHeight;
            track.style.backgroundColor = Settings.ProgressTrack;
            SetRadius(track, Settings.ProgressRadius);
            track.style.marginTop = 16;

            if (Settings.ProgressAtBottom)
            {
                track.style.position = Position.Absolute;
                track.style.bottom = 24;
            }

            VisualElement fill = new VisualElement();
            fill.style.width = Length.Percent(_previewProgress * 100f);
            fill.style.height = Length.Percent(100);
            fill.style.backgroundColor = Settings.ProgressFill;
            SetRadius(fill, Settings.ProgressRadius);
            track.Add(fill);
            frame.Add(track);

            if (string.IsNullOrEmpty(Settings.LoadingText) == false)
            {
                Label text = TextLabel(Settings.LoadingText, "jtl-text");
                text.style.marginTop = 10;
                frame.Add(text);
            }

            card.Add(frame);

            Slider progress = new Slider(0f, 1f) { value = _previewProgress };
            progress.RegisterValueChangedCallback(changeEvent =>
            {
                _previewProgress = changeEvent.newValue;
                fill.style.width = Length.Percent(_previewProgress * 100f);
            });
            card.Add(Field("template.progress", progress));
            return card;
        }

        private Color PreviewColor(TemplateBackground background)
        {
            return background.Kind == BackgroundKind.Gradient ? Color.Lerp(background.GradientFrom, background.GradientTo, 0.5f) : background.Color;
        }

        private void SetScaleMode(VisualElement element, ScaleMode mode)
        {
#if UNITY_2022_2_OR_NEWER
            element.style.backgroundPositionX = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(mode);
            element.style.backgroundPositionY = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(mode);
            element.style.backgroundRepeat = BackgroundPropertyHelper.ConvertScaleModeToBackgroundRepeat(mode);
            element.style.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize(mode);
#else
            element.style.unityBackgroundScaleMode = mode;
#endif
        }

        private void SetRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        private FieldRow Field(string labelKey, VisualElement control)
        {
            FieldRow row = new FieldRow(labelKey, LabelWidth);
            row.Add(control);
            return row;
        }

        private ObjectField TextureInput(Texture2D value, Action<Texture2D> assign)
        {
            ObjectField field = new ObjectField { objectType = typeof(Texture2D), allowSceneObjects = false, value = value };
            field.AddToClassList("jtl-object-field");
            field.style.width = 260;
            field.RegisterValueChangedCallback(changeEvent => Change(() => assign(changeEvent.newValue as Texture2D)));
            return field;
        }

        private ColorField ColorInput(Color value, Action<Color> assign)
        {
            ColorField field = new ColorField { value = value, showAlpha = false };
            field.AddToClassList("jtl-color-field");
            field.style.width = 160;
            field.RegisterCallback<FocusOutEvent>(focusEvent => Change(() => assign(field.value)));
            field.RegisterValueChangedCallback(changeEvent =>
            {
                assign(changeEvent.newValue);
                Settings.Persist();
            });
            return field;
        }

        private NumberFieldWithUnit IntegerInput(int value, string unitKey, Action<int> assign)
        {
            NumberFieldWithUnit field = new NumberFieldWithUnit { UnitKey = unitKey, Width = 90 };
            field.Value = value.ToString(CultureInfo.InvariantCulture);
            field.Input.RegisterCallback<FocusOutEvent>(focusEvent =>
            {
                bool valid = int.TryParse(field.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed);
                field.Error = valid == false;

                if (valid && parsed != value)
                {
                    Change(() => assign(parsed));
                }
            });
            return field;
        }

        private TextField TextInput(string value, Action<string> assign)
        {
            TextField field = new TextField { value = value };
            field.AddToClassList("jtl-field");
            field.AddToClassList("jtl-grow");
            field.style.minWidth = 0;
            field.RegisterCallback<FocusOutEvent>(focusEvent =>
            {
                if (field.value != value)
                {
                    Change(() => assign(field.value));
                }
            });
            return field;
        }

        private void Install()
        {
            _template.Install();
            Context.Report(StatusKind.Success, "template.installedTo", TemplateService.TemplateFolder);
            Render();
        }

        private void Change(Action change)
        {
            change();
            Settings.Persist();
            Render();
        }
    }
}
