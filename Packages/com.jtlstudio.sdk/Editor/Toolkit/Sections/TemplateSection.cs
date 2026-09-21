using System;
using System.Collections.Generic;
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
        private const int LabelWidth = FieldRow.DefaultLabelWidth;
        private const int ControlWidth = 240;
        private const int SwatchWidth = 160;
        private const int NumberWidth = 96;
        private const int PreviewWidth = 400;
        private const int PreviewPadding = 34;
        private const int MobilePreviewWidth = 240;
        private const int DesktopPreviewHeight = 250;
        private const int MobilePreviewHeight = 460;
        private const int ColumnGap = 12;
        private const int MinimumSettingsWidth = 440;
        private const int GradientResolution = 160;
        private const float PreviewScale = 0.5f;

        private readonly TemplateService _template = new TemplateService();
        private bool _mobilePreview;
        private float _previewProgress = 0.55f;
        private Texture2D _gradient;
        private VisualElement _frame;
        private VisualElement _logo;
        private VisualElement _track;
        private VisualElement _fill;
        private Label _loadingText;

        public TemplateSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Template;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "TemplateSection";

        private JTLSDKEditorSettings Settings => JTLSDKEditorSettings.instance;

        private int FrameWidth => _mobilePreview ? MobilePreviewWidth : PreviewWidth;

        private int FrameHeight => _mobilePreview ? MobilePreviewHeight : DesktopPreviewHeight;

        protected override void OnRendered()
        {
            SectionHeader header = Require<SectionHeader>("template-header");
            VisualElement body = Require<VisualElement>("template-body");
            _frame = null;

            if (_template.IsInstalled == false)
            {
                EmptyState empty = new EmptyState { TitleKey = "template.notInstalled", IconName = "template" };
                empty.style.flexGrow = 1;
                empty.Add(Button("template.install", ToolkitButton.PrimaryVariant, "plus", Install));
                body.Add(empty);
                return;
            }

            header.Add(new Badge("template.installed", Badge.SuccessVariant));
            header.Add(Button("template.reinstall", ToolkitButton.GhostVariant, "refresh", Install));
            header.Add(Button("template.remove", ToolkitButton.GhostVariant, "delete", Uninstall));

            VisualElement settings = Column(12);
            settings.AddToClassList("jtl-basis");
            settings.style.minWidth = 0;
            settings.Add(CreateLogoCard());
            settings.Add(CreateBackgroundCard("template.loadingScreen", Settings.LoaderBackground));
            settings.Add(CreateProgressCard());
            settings.Add(CreateBackgroundCard("template.pageBackground", Settings.PageBackground));
            settings.Add(CreateCanvasCard());
            body.Add(settings);
            VisualElement preview = CreatePreviewCard();
            body.Add(preview);
            body.RegisterCallback<GeometryChangedEvent>(geometryEvent => ArrangeColumns(body, settings, preview));
            RefreshPreview();
        }

        private void ArrangeColumns(VisualElement body, VisualElement settings, VisualElement preview)
        {
            bool stacked = body.resolvedStyle.width < MinimumSettingsWidth + ColumnGap + PreviewWidth + PreviewPadding;
            FlexDirection direction = stacked ? FlexDirection.Column : FlexDirection.Row;

            if (body.resolvedStyle.flexDirection == direction)
            {
                return;
            }

            body.style.flexDirection = direction;
            settings.style.flexBasis = stacked ? new StyleLength(StyleKeyword.Auto) : new StyleLength(0f);
            settings.style.flexGrow = stacked ? 0 : 1;
            settings.style.alignSelf = stacked ? Align.Stretch : Align.Auto;
            preview.style.marginLeft = stacked ? 0 : ColumnGap;
            preview.style.marginTop = stacked ? ColumnGap : 0;
        }

        private VisualElement CreateLogoCard()
        {
            Card card = new Card { TitleKey = "template.logo" };
            card.Add(Field("template.logoSource", Segments("template.logoModes", (int)Settings.LogoMode, index => Rebuild(() => Settings.LogoMode = (LogoMode)index))));

            if (Settings.LogoMode == LogoMode.Custom)
            {
                card.Add(Field("template.logoFile", TextureInput(Settings.Logo, texture => Settings.Logo = texture)));
            }

            if (Settings.LogoMode != LogoMode.None)
            {
                card.Add(Field("template.logoSize", IntegerInput(() => Settings.LogoSize, value => Settings.LogoSize = value, "unit.px")));
            }

            return card;
        }

        private VisualElement CreateBackgroundCard(string titleKey, TemplateBackground background)
        {
            Card card = new Card { TitleKey = titleKey };
            card.Add(Field("template.background", Segments("template.backgroundKinds", (int)background.Kind, index => Rebuild(() => background.Kind = (BackgroundKind)index))));

            switch (background.Kind)
            {
                case BackgroundKind.Gradient:
                    card.Add(Field("template.gradientFrom", ColorInput(background.GradientFrom, color => background.GradientFrom = color)));
                    card.Add(Field("template.gradientTo", ColorInput(background.GradientTo, color => background.GradientTo = color)));
                    card.Add(Field("template.gradientShape", Segments("template.gradientShapes", background.Radial ? 1 : 0, index => Rebuild(() => background.Radial = index == 1))));

                    if (background.Radial == false)
                    {
                        card.Add(Field("template.angle", IntegerInput(() => background.Angle, value => background.Angle = value, "unit.deg")));
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

            return card;
        }

        private VisualElement CreateProgressCard()
        {
            Card card = new Card { TitleKey = "template.progressBar" };
            card.Add(Field("template.fill", ColorInput(Settings.ProgressFill, color => Settings.ProgressFill = color)));
            card.Add(Field("template.track", ColorInput(Settings.ProgressTrack, color => Settings.ProgressTrack = color)));
            card.Add(Field("template.progressWidth", IntegerInput(() => Settings.ProgressWidthPercent, value => Settings.ProgressWidthPercent = value, "unit.percent")));
            card.Add(Field("template.progressHeight", IntegerInput(() => Settings.ProgressHeight, value => Settings.ProgressHeight = value, "unit.px")));
            card.Add(Field("template.progressRadius", IntegerInput(() => Settings.ProgressRadius, value => Settings.ProgressRadius = value, "unit.px")));
            card.Add(Field("template.progressPosition", Segments("template.positions", Settings.ProgressAtBottom ? 1 : 0, index => Update(() => Settings.ProgressAtBottom = index == 1))));
            card.Add(Field("template.loadingText", TextInput(Settings.LoadingText, value => Settings.LoadingText = value)));
            return card;
        }

        private VisualElement CreateCanvasCard()
        {
            Card card = new Card { TitleKey = "template.canvas" };
            card.Add(Field("template.fixedAspect", Switch(Settings.FixedAspect, value => Rebuild(() => Settings.FixedAspect = value))));

            if (Settings.FixedAspect)
            {
                TextField ratio = TextInput(Settings.AspectRatio, value => Settings.AspectRatio = value);
                ratio.style.maxWidth = NumberWidth;
                card.Add(Field("template.aspectRatio", ratio));
                card.Add(Field("template.freeOnMobile", Switch(Settings.FreeAspectOnMobile, value => Update(() => Settings.FreeAspectOnMobile = value))));
            }

            card.Add(Field("template.pixelRatioDesktop", PixelRatioInput(Settings.DesktopPixelRatioMode, () => Settings.DesktopPixelRatio, mode => Settings.DesktopPixelRatioMode = mode, value => Settings.DesktopPixelRatio = value)));
            card.Add(Field("template.pixelRatioMobile", PixelRatioInput(Settings.MobilePixelRatioMode, () => Settings.MobilePixelRatio, mode => Settings.MobilePixelRatioMode = mode, value => Settings.MobilePixelRatio = value)));
            card.Add(Field("template.fullscreenButton", Switch(Settings.FullscreenButton, value => Update(() => Settings.FullscreenButton = value))));
            return card;
        }

        private VisualElement PixelRatioInput(PixelRatioMode mode, Func<float> read, Action<PixelRatioMode> assignMode, Action<float> assignValue)
        {
            VisualElement row = Row(8);
            row.style.flexGrow = 1;
            row.style.minWidth = 0;
            Dropdown modes = new Dropdown();
            modes.style.width = 180;
            modes.style.flexShrink = 1;
            modes.style.minWidth = 0;
            modes.choices = new List<string>(Context.Localization.GetList("template.pixelRatioModes"));
            modes.index = (int)mode;
            modes.RegisterValueChangedCallback(changeEvent => Rebuild(() => assignMode((PixelRatioMode)modes.index)));
            row.Add(modes);

            if (mode != PixelRatioMode.Auto)
            {
                NumberFieldWithUnit number = new NumberFieldWithUnit { Width = 72 };
                number.Value = read().ToString("0.##", CultureInfo.InvariantCulture);
                number.Input.RegisterCallback<FocusOutEvent>(focusEvent =>
                {
                    bool valid = float.TryParse(number.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed);
                    number.Error = valid == false;

                    if (valid)
                    {
                        Update(() => assignValue(parsed));
                        number.Value = read().ToString("0.##", CultureInfo.InvariantCulture);
                    }
                });
                row.Add(number);
            }

            return row;
        }

        private VisualElement CreatePreviewCard()
        {
            Card card = new Card { TitleKey = "template.preview" };
            card.style.width = PreviewWidth + PreviewPadding;
            card.style.flexShrink = 0;
            card.style.alignSelf = Align.FlexStart;
            card.style.marginLeft = ColumnGap;

            SegmentedControl device = new SegmentedControl();
            device.SetChoices(Context.Localization.GetList("template.previewDevices"));
            device.Index = _mobilePreview ? 1 : 0;
            device.IndexChanged += index =>
            {
                _mobilePreview = index == 1;
                Render();
            };
            card.Header.Add(device);

            _frame = new VisualElement();
            _frame.AddToClassList("jtl-template-preview");
            _frame.style.width = FrameWidth;
            _frame.style.height = FrameHeight;
            _frame.style.alignSelf = Align.Center;

            _logo = new VisualElement();
            SetScaleMode(_logo, ScaleMode.ScaleToFit);
            _frame.Add(_logo);

            _track = new VisualElement();
            _track.AddToClassList("jtl-template-preview__track");
            _fill = new VisualElement();
            _fill.style.height = Length.Percent(100);
            _track.Add(_fill);
            _frame.Add(_track);

            _loadingText = TextLabel("", "jtl-text");
            _loadingText.style.marginTop = 10;
            _frame.Add(_loadingText);
            card.Add(_frame);

            Slider progress = new Slider(0f, 1f) { value = _previewProgress };
            progress.style.flexGrow = 1;
            progress.RegisterValueChangedCallback(changeEvent =>
            {
                _previewProgress = changeEvent.newValue;
                _fill.style.width = Length.Percent(_previewProgress * 100f);
            });
            FieldRow progressRow = new FieldRow("template.progress", 96);
            progressRow.Add(progress);
            card.Add(progressRow);
            return card;
        }

        private void RefreshPreview()
        {
            if (_frame == null)
            {
                return;
            }

            ApplyBackground(_frame, Settings.LoaderBackground, FrameWidth, FrameHeight);

            Texture2D logo = _template.LogoTexture(Settings);
            _logo.style.display = Settings.LogoMode == LogoMode.None ? DisplayStyle.None : DisplayStyle.Flex;
            float logoWidth = Mathf.Min(Settings.LogoSize * PreviewScale, FrameWidth * 0.6f);
            _logo.style.width = logoWidth;
            _logo.style.height = logo == null ? logoWidth * 0.5f : logoWidth * logo.height / Mathf.Max(1f, logo.width);
            _logo.style.backgroundImage = logo;
            _logo.EnableInClassList("jtl-template-preview__logo-placeholder", logo == null);

            _track.style.width = Length.Percent(Settings.ProgressWidthPercent);
            _track.style.height = Mathf.Max(1, Settings.ProgressHeight);
            _track.style.backgroundColor = Settings.ProgressTrack;
            _track.style.position = Settings.ProgressAtBottom ? Position.Absolute : Position.Relative;
            _track.style.bottom = Settings.ProgressAtBottom ? new StyleLength(24f) : new StyleLength(StyleKeyword.Auto);
            _track.style.marginTop = Settings.ProgressAtBottom ? 0 : 16;
            SetRadius(_track, Settings.ProgressRadius);
            _fill.style.width = Length.Percent(_previewProgress * 100f);
            _fill.style.backgroundColor = Settings.ProgressFill;
            SetRadius(_fill, Settings.ProgressRadius);

            _loadingText.text = Settings.LoadingText;
            _loadingText.style.display = string.IsNullOrEmpty(Settings.LoadingText) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void ApplyBackground(VisualElement target, TemplateBackground background, int width, int height)
        {
            target.style.backgroundColor = background.Color;
            target.style.backgroundImage = StyleKeyword.None;

            if (background.Kind == BackgroundKind.Gradient)
            {
                target.style.backgroundImage = GradientTexture(background, width, height);
                SetScaleMode(target, ScaleMode.StretchToFill);
                return;
            }

            if (background.Kind == BackgroundKind.Image && background.Image != null)
            {
                target.style.backgroundImage = background.Image;
                SetScaleMode(target, ScaleMode.ScaleAndCrop);
            }
        }

        private Texture2D GradientTexture(TemplateBackground background, int width, int height)
        {
            int textureWidth = GradientResolution;
            int textureHeight = Mathf.Max(1, Mathf.RoundToInt(GradientResolution * height / (float)Mathf.Max(1, width)));

            if (_gradient == null || _gradient.width != textureWidth || _gradient.height != textureHeight)
            {
                if (_gradient != null)
                {
                    UnityEngine.Object.DestroyImmediate(_gradient);
                }

                _gradient = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            float radians = background.Angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
            float halfLength = Mathf.Max(0.0001f, (Mathf.Abs(textureWidth * direction.x) + Mathf.Abs(textureHeight * direction.y)) * 0.5f);
            float radius = Mathf.Max(0.0001f, Mathf.Sqrt(textureWidth * textureWidth + textureHeight * textureHeight) * 0.5f);
            Color[] pixels = new Color[textureWidth * textureHeight];

            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f - textureWidth * 0.5f, y + 0.5f - textureHeight * 0.5f);
                    float position = background.Radial ? point.magnitude / radius : (Vector2.Dot(point, direction) / halfLength + 1f) * 0.5f;
                    pixels[y * textureWidth + x] = Color.Lerp(background.GradientFrom, background.GradientTo, Mathf.Clamp01(position));
                }
            }

            _gradient.SetPixels(pixels);
            _gradient.Apply(false);
            return _gradient;
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

        private SegmentedControl Segments(string choicesKey, int index, Action<int> onChanged)
        {
            SegmentedControl control = new SegmentedControl();
            control.SetChoices(Context.Localization.GetList(choicesKey));
            control.Index = index;
            control.IndexChanged += onChanged;
            return control;
        }

        private SwitchToggle Switch(bool value, Action<bool> onChanged)
        {
            SwitchToggle toggle = new SwitchToggle(value);
            toggle.ValueChanged += onChanged;
            return toggle;
        }

        private ObjectField TextureInput(Texture2D value, Action<Texture2D> assign)
        {
            ObjectField field = new ObjectField { objectType = typeof(Texture2D), allowSceneObjects = false, value = value };
            field.AddToClassList("jtl-object-field");
            field.style.maxWidth = ControlWidth;
            field.style.flexGrow = 1;
            field.style.flexShrink = 1;
            field.style.minWidth = 0;
            field.RegisterValueChangedCallback(changeEvent => Update(() => assign(changeEvent.newValue as Texture2D)));
            return field;
        }

        private ColorSwatchField ColorInput(Color value, Action<Color> assign)
        {
            ColorSwatchField field = new ColorSwatchField { Value = value };
            field.style.width = SwatchWidth;
            field.ValueChanged += color => Update(() => assign(color));
            return field;
        }

        private NumberFieldWithUnit IntegerInput(Func<int> read, Action<int> assign, string unitKey)
        {
            NumberFieldWithUnit field = new NumberFieldWithUnit { UnitKey = unitKey, Width = NumberWidth };
            field.Value = read().ToString(CultureInfo.InvariantCulture);
            field.Input.RegisterCallback<FocusOutEvent>(focusEvent =>
            {
                bool valid = int.TryParse(field.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed);
                field.Error = valid == false;

                if (valid)
                {
                    Update(() => assign(parsed));
                    field.Value = read().ToString(CultureInfo.InvariantCulture);
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
            field.style.maxWidth = ControlWidth;
            field.RegisterCallback<FocusOutEvent>(focusEvent => Update(() => assign(field.value)));
            return field;
        }

        private void Install()
        {
            _template.Install();
            Context.Report(StatusKind.Success, "template.installedTo", TemplateService.TemplateFolder);
            Render();
        }

        private void Uninstall()
        {
            if (Context.Confirm("template.removeTitle", "template.removeMessage", "template.remove") == false)
            {
                return;
            }

            _template.Uninstall();
            Context.Report(StatusKind.Info, "template.removed");
            Render();
        }

        private void Update(Action change)
        {
            change();
            Settings.Persist();
            RefreshPreview();
        }

        private void Rebuild(Action change)
        {
            change();
            Settings.Persist();
            Render();
        }
    }
}
