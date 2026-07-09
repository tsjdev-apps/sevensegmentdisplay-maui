using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

namespace MauiSevenSegmentDisplay.Controls;

/// <summary>
/// Reusable SkiaSharp-powered MAUI control that draws text with classic seven-segment geometry.
/// </summary>
public sealed class SevenSegmentDisplay : SKCanvasView
{
    // Drawing in a stable order keeps inactive and active layers predictable.
    private static readonly SevenSegmentParts[] SegmentOrder =
    [
        SevenSegmentParts.A,
        SevenSegmentParts.B,
        SevenSegmentParts.C,
        SevenSegmentParts.D,
        SevenSegmentParts.E,
        SevenSegmentParts.F,
        SevenSegmentParts.G
    ];

    // A 60 FPS-ish timer is enough for the short glow pulse without creating animation overhead.
    private static readonly TimeSpan AnimationFrameInterval = TimeSpan.FromMilliseconds(16);

    // The animation timer is owned by the control so it does not depend on MAUI's animation manager.
    private IDispatcherTimer? _textAnimationTimer;
    private DateTimeOffset _animationStartedAt;
    private float _animationProgress = 1f;

    /// <summary>
    /// Backing bindable property for <see cref="Text"/>.
    /// </summary>
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(SevenSegmentDisplay), string.Empty, propertyChanged: OnTextChanged);

    /// <summary>
    /// Backing bindable property for <see cref="SegmentColor"/>.
    /// </summary>
    public static readonly BindableProperty SegmentColorProperty =
        BindableProperty.Create(nameof(SegmentColor), typeof(Color), typeof(SevenSegmentDisplay), Colors.Lime, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="InactiveSegmentColor"/>.
    /// </summary>
    public static readonly BindableProperty InactiveSegmentColorProperty =
        BindableProperty.Create(nameof(InactiveSegmentColor), typeof(Color), typeof(SevenSegmentDisplay), Color.FromArgb("#1f5a32"), propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="DisplayBackgroundColor"/>.
    /// </summary>
    public static readonly BindableProperty DisplayBackgroundColorProperty =
        BindableProperty.Create(nameof(DisplayBackgroundColor), typeof(Color), typeof(SevenSegmentDisplay), Colors.Black, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="GlowColor"/>.
    /// </summary>
    public static readonly BindableProperty GlowColorProperty =
        BindableProperty.Create(nameof(GlowColor), typeof(Color), typeof(SevenSegmentDisplay), Colors.Lime, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="SegmentThickness"/>.
    /// </summary>
    public static readonly BindableProperty SegmentThicknessProperty =
        BindableProperty.Create(nameof(SegmentThickness), typeof(double), typeof(SevenSegmentDisplay), 12d, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="CharacterSpacing"/>.
    /// </summary>
    public static readonly BindableProperty CharacterSpacingProperty =
        BindableProperty.Create(nameof(CharacterSpacing), typeof(double), typeof(SevenSegmentDisplay), 8d, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="SegmentCornerRadius"/>.
    /// </summary>
    public static readonly BindableProperty SegmentCornerRadiusProperty =
        BindableProperty.Create(nameof(SegmentCornerRadius), typeof(double), typeof(SevenSegmentDisplay), 4d, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="InactiveSegmentOpacity"/>.
    /// </summary>
    public static readonly BindableProperty InactiveSegmentOpacityProperty =
        BindableProperty.Create(nameof(InactiveSegmentOpacity), typeof(double), typeof(SevenSegmentDisplay), 0.28d, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="GlowIntensity"/>.
    /// </summary>
    public static readonly BindableProperty GlowIntensityProperty =
        BindableProperty.Create(nameof(GlowIntensity), typeof(double), typeof(SevenSegmentDisplay), 0.7d, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="IsGlowEnabled"/>.
    /// </summary>
    public static readonly BindableProperty IsGlowEnabledProperty =
        BindableProperty.Create(nameof(IsGlowEnabled), typeof(bool), typeof(SevenSegmentDisplay), true, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="IsAnimationEnabled"/>.
    /// </summary>
    public static readonly BindableProperty IsAnimationEnabledProperty =
        BindableProperty.Create(nameof(IsAnimationEnabled), typeof(bool), typeof(SevenSegmentDisplay), true);

    /// <summary>
    /// Backing bindable property for <see cref="AnimationDuration"/>.
    /// </summary>
    public static readonly BindableProperty AnimationDurationProperty =
        BindableProperty.Create(nameof(AnimationDuration), typeof(uint), typeof(SevenSegmentDisplay), 250u);

    /// <summary>
    /// Backing bindable property for <see cref="HorizontalTextAlignment"/>.
    /// </summary>
    public static readonly BindableProperty HorizontalTextAlignmentProperty =
        BindableProperty.Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(SevenSegmentDisplay), TextAlignment.Center, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Backing bindable property for <see cref="VerticalTextAlignment"/>.
    /// </summary>
    public static readonly BindableProperty VerticalTextAlignmentProperty =
        BindableProperty.Create(nameof(VerticalTextAlignment), typeof(TextAlignment), typeof(SevenSegmentDisplay), TextAlignment.Center, propertyChanged: OnVisualPropertyChanged);

    /// <summary>
    /// Initializes the canvas view and hooks cleanup for the optional animation timer.
    /// </summary>
    public SevenSegmentDisplay()
    {
        EnableTouchEvents = false;
        IgnorePixelScaling = false;
        Unloaded += (_, _) => StopTextAnimation();
    }

    /// <summary>
    /// Gets or sets the text rendered by the seven-segment display.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the color used for active segments.
    /// </summary>
    public Color SegmentColor
    {
        get => (Color)GetValue(SegmentColorProperty);
        set => SetValue(SegmentColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the base color used for inactive segments before opacity is applied.
    /// </summary>
    public Color InactiveSegmentColor
    {
        get => (Color)GetValue(InactiveSegmentColorProperty);
        set => SetValue(InactiveSegmentColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the display face background color.
    /// </summary>
    public Color DisplayBackgroundColor
    {
        get => (Color)GetValue(DisplayBackgroundColorProperty);
        set => SetValue(DisplayBackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color used for the blurred glow behind active segments.
    /// </summary>
    public Color GlowColor
    {
        get => (Color)GetValue(GlowColorProperty);
        set => SetValue(GlowColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the design-space thickness of each segment.
    /// </summary>
    public double SegmentThickness
    {
        get => (double)GetValue(SegmentThicknessProperty);
        set => SetValue(SegmentThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the design-space spacing between rendered characters.
    /// </summary>
    public double CharacterSpacing
    {
        get => (double)GetValue(CharacterSpacingProperty);
        set => SetValue(CharacterSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius used when rounding segment polygons.
    /// </summary>
    public double SegmentCornerRadius
    {
        get => (double)GetValue(SegmentCornerRadiusProperty);
        set => SetValue(SegmentCornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the opacity multiplier applied to inactive segments.
    /// </summary>
    public double InactiveSegmentOpacity
    {
        get => (double)GetValue(InactiveSegmentOpacityProperty);
        set => SetValue(InactiveSegmentOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets the strength of the active segment glow.
    /// </summary>
    public double GlowIntensity
    {
        get => (double)GetValue(GlowIntensityProperty);
        set => SetValue(GlowIntensityProperty, value);
    }

    /// <summary>
    /// Gets or sets whether active segments draw a blurred glow.
    /// </summary>
    public bool IsGlowEnabled
    {
        get => (bool)GetValue(IsGlowEnabledProperty);
        set => SetValue(IsGlowEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether text changes trigger a short pulse animation.
    /// </summary>
    public bool IsAnimationEnabled
    {
        get => (bool)GetValue(IsAnimationEnabledProperty);
        set => SetValue(IsAnimationEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the text-change animation duration in milliseconds.
    /// </summary>
    public uint AnimationDuration
    {
        get => (uint)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets how the full rendered text is aligned horizontally inside the canvas.
    /// </summary>
    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets how the full rendered text is aligned vertically inside the canvas.
    /// </summary>
    public TextAlignment VerticalTextAlignment
    {
        get => (TextAlignment)GetValue(VerticalTextAlignmentProperty);
        set => SetValue(VerticalTextAlignmentProperty, value);
    }

    /// <summary>
    /// Draws the current text by scaling the seven-segment design model to the Skia canvas.
    /// </summary>
    protected override void OnPaintSurface(SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        SKCanvas canvas = e.Surface.Canvas;
        SKImageInfo info = e.Info;
        canvas.Clear(ToSkColor(DisplayBackgroundColor));

        string text = Text ?? string.Empty;
        if (string.IsNullOrEmpty(text) || info.Width <= 0 || info.Height <= 0)
        {
            return;
        }

        float designThickness = MathF.Max((float)SegmentThickness, SevenSegmentOptions.MinimumDesignThickness);
        float designSpacing = MathF.Max(0, (float)CharacterSpacing);
        float characterWidth = designThickness * SevenSegmentOptions.DefaultCharacterWidthRatio;
        float characterHeight = designThickness * SevenSegmentOptions.DefaultCharacterHeightRatio;
        float symbolWidth = designThickness * SevenSegmentOptions.SymbolWidthRatio;

        // Scaling starts with a stable design-space model, then fits the whole
        // string inside the actual high-DPI Skia pixel canvas.
        float totalWidth = MeasureText(text, characterWidth, symbolWidth, designSpacing);
        float availableWidth = MathF.Max(1, info.Width - 24);
        float availableHeight = MathF.Max(1, info.Height - 20);
        float scale = MathF.Min(availableWidth / totalWidth, availableHeight / characterHeight);

        float scaledWidth = totalWidth * scale;
        float scaledHeight = characterHeight * scale;
        SKPoint origin = GetAlignedOrigin(info.Width, info.Height, scaledWidth, scaledHeight);

        canvas.Save();
        canvas.Translate(origin.X, origin.Y);
        canvas.Scale(scale);

        using SKPaint inactivePaint = CreateFillPaint(ToSkColor(InactiveSegmentColor, Clamp01((float)InactiveSegmentOpacity)));
        using SKPaint activePaint = CreateFillPaint(ToSkColor(SegmentColor));
        using SKPaint? glowPaint = CreateGlowPaint();

        float pulse = IsAnimationEnabled ? MathF.Sin(MathF.PI * _animationProgress) * 0.35f : 0f;
        float glowScale = 1f + pulse;
        float x = 0f;

        foreach (char character in text)
        {
            float width = IsSymbol(character) ? symbolWidth : characterWidth;

            if (character == ':')
            {
                DrawColon(canvas, x, characterHeight, designThickness, inactivePaint, activePaint, glowPaint, glowScale);
            }
            else if (character == '.')
            {
                DrawDot(canvas, x, characterHeight, designThickness, inactivePaint, activePaint, glowPaint, glowScale);
            }
            else
            {
                DrawCharacter(canvas, character, x, 0, characterWidth, characterHeight, designThickness, inactivePaint, activePaint, glowPaint, glowScale);
            }

            x += width + designSpacing;
        }

        canvas.Restore();
    }

    /// <summary>
    /// Invalidates the Skia surface when any visual bindable property changes.
    /// </summary>
    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((SevenSegmentDisplay)bindable).InvalidateSurface();
    }

    /// <summary>
    /// Starts the optional pulse animation when the displayed text changes.
    /// </summary>
    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        SevenSegmentDisplay display = (SevenSegmentDisplay)bindable;

        // Text changes trigger a lightweight pulse. The segment map is always
        // drawn from the current text, while glow/brightness briefly swell.
        display.StartTextAnimation();
    }

    /// <summary>
    /// Starts a lightweight dispatcher-timer animation after the control is loaded.
    /// </summary>
    private void StartTextAnimation()
    {
        StopTextAnimation();

        // Initial bindings can run before MAUI attaches an animation manager or
        // handler to the view. Rendering immediately here avoids startup crashes,
        // while later text changes still get the pulse animation.
        if (!IsAnimationEnabled || AnimationDuration == 0 || !IsLoaded || Handler is null || Dispatcher is null)
        {
            _animationProgress = 1f;
            InvalidateSurface();
            return;
        }

        _animationProgress = 0f;
        _animationStartedAt = DateTimeOffset.UtcNow;
        _textAnimationTimer = Dispatcher.CreateTimer();
        _textAnimationTimer.Interval = AnimationFrameInterval;
        _textAnimationTimer.Tick += OnTextAnimationTick;
        _textAnimationTimer.Start();
        InvalidateSurface();
    }

    /// <summary>
    /// Stops and detaches the dispatcher timer used by the text-change pulse.
    /// </summary>
    private void StopTextAnimation()
    {
        if (_textAnimationTimer is null)
        {
            return;
        }

        _textAnimationTimer.Stop();
        _textAnimationTimer.Tick -= OnTextAnimationTick;
        _textAnimationTimer = null;
    }

    /// <summary>
    /// Advances the pulse animation and redraws the canvas for the current frame.
    /// </summary>
    private void OnTextAnimationTick(object? sender, EventArgs e)
    {
        TimeSpan elapsed = DateTimeOffset.UtcNow - _animationStartedAt;
        TimeSpan duration = TimeSpan.FromMilliseconds(AnimationDuration);
        double progress = duration.TotalMilliseconds <= 0 ? 1d : elapsed.TotalMilliseconds / duration.TotalMilliseconds;

        _animationProgress = (float)Math.Clamp(Easing.CubicOut.Ease(progress), 0d, 1d);
        InvalidateSurface();

        if (_animationProgress >= 1f)
        {
            StopTextAnimation();
        }
    }

    /// <summary>
    /// Draws one seven-segment character, including inactive bars, active bars, and optional glow.
    /// </summary>
    private void DrawCharacter(
        SKCanvas canvas,
        char character,
        float x,
        float y,
        float width,
        float height,
        float thickness,
        SKPaint inactivePaint,
        SKPaint activePaint,
        SKPaint? glowPaint,
        float glowScale)
    {
        SevenSegmentParts activeSegments = SegmentCharacterMap.GetSegments(character);

        foreach (SevenSegmentParts segment in SegmentOrder)
        {
            using SKPath path = CreateSegmentPath(segment, x, y, width, height, thickness);
            canvas.DrawPath(path, inactivePaint);

            if (!activeSegments.HasFlag(segment))
            {
                continue;
            }

            if (glowPaint is not null)
            {
                glowPaint.Color = WithAlpha(glowPaint.Color, Clamp01((float)GlowIntensity * glowScale));
                canvas.DrawPath(path, glowPaint);
            }

            canvas.DrawPath(path, activePaint);
        }
    }

    /// <summary>
    /// Draws the two dots used by clock-style colon separators.
    /// </summary>
    private void DrawColon(
        SKCanvas canvas,
        float x,
        float height,
        float thickness,
        SKPaint inactivePaint,
        SKPaint activePaint,
        SKPaint? glowPaint,
        float glowScale)
    {
        float radius = thickness * 0.42f;
        float centerX = x + thickness * 1.1f;
        DrawRoundLight(canvas, centerX, height * 0.36f, radius, inactivePaint, activePaint, glowPaint, glowScale);
        DrawRoundLight(canvas, centerX, height * 0.64f, radius, inactivePaint, activePaint, glowPaint, glowScale);
    }

    /// <summary>
    /// Draws a single decimal-style dot in the lower-right area of a symbol slot.
    /// </summary>
    private void DrawDot(
        SKCanvas canvas,
        float x,
        float height,
        float thickness,
        SKPaint inactivePaint,
        SKPaint activePaint,
        SKPaint? glowPaint,
        float glowScale)
    {
        float radius = thickness * 0.45f;
        DrawRoundLight(canvas, x + thickness * 1.1f, height - thickness * 0.8f, radius, inactivePaint, activePaint, glowPaint, glowScale);
    }

    /// <summary>
    /// Draws a circular symbol light with the same inactive/active/glow layering as normal segments.
    /// </summary>
    private void DrawRoundLight(
        SKCanvas canvas,
        float centerX,
        float centerY,
        float radius,
        SKPaint inactivePaint,
        SKPaint activePaint,
        SKPaint? glowPaint,
        float glowScale)
    {
        canvas.DrawCircle(centerX, centerY, radius, inactivePaint);

        if (glowPaint is not null)
        {
            glowPaint.Color = WithAlpha(glowPaint.Color, Clamp01((float)GlowIntensity * glowScale));
            canvas.DrawCircle(centerX, centerY, radius, glowPaint);
        }

        canvas.DrawCircle(centerX, centerY, radius, activePaint);
    }

    /// <summary>
    /// Builds the polygon path for one named segment in the current character box.
    /// </summary>
    private SKPath CreateSegmentPath(SevenSegmentParts segment, float x, float y, float width, float height, float thickness)
    {
        float half = thickness / 2f;
        float topY = y + thickness * 0.15f;
        float middleY = y + (height - thickness) / 2f;
        float bottomY = y + height - thickness * 1.15f;
        float upperHeight = middleY - topY;
        float lowerHeight = bottomY - middleY;

        SKPoint[] points = segment switch
        {
            SevenSegmentParts.A => Horizontal(x + half, topY, width - thickness, thickness),
            SevenSegmentParts.G => Horizontal(x + half, middleY, width - thickness, thickness),
            SevenSegmentParts.D => Horizontal(x + half, bottomY, width - thickness, thickness),
            SevenSegmentParts.B => Vertical(x + width - thickness * 1.05f, topY + half, thickness, upperHeight),
            SevenSegmentParts.C => Vertical(x + width - thickness * 1.05f, middleY + half, thickness, lowerHeight),
            SevenSegmentParts.E => Vertical(x + thickness * 0.05f, middleY + half, thickness, lowerHeight),
            SevenSegmentParts.F => Vertical(x + thickness * 0.05f, topY + half, thickness, upperHeight),
            _ => []
        };

        // Each bar is a beveled polygon, then the corners are rounded by walking
        // each vertex and replacing the hard turn with a small quadratic curve.
        return CreateRoundedPolygon(points, MathF.Max(0, (float)SegmentCornerRadius));
    }

    /// <summary>
    /// Creates the beveled polygon points for a horizontal segment.
    /// </summary>
    private static SKPoint[] Horizontal(float x, float y, float width, float thickness)
    {
        float half = thickness / 2f;
        return
        [
            new(x, y + half),
            new(x + half, y),
            new(x + width - half, y),
            new(x + width, y + half),
            new(x + width - half, y + thickness),
            new(x + half, y + thickness)
        ];
    }

    /// <summary>
    /// Creates the beveled polygon points for a vertical segment.
    /// </summary>
    private static SKPoint[] Vertical(float x, float y, float thickness, float height)
    {
        float half = thickness / 2f;
        return
        [
            new(x + half, y),
            new(x + thickness, y + half),
            new(x + thickness, y + height - half),
            new(x + half, y + height),
            new(x, y + height - half),
            new(x, y + half)
        ];
    }

    /// <summary>
    /// Rounds a polygon by replacing each sharp vertex with a short quadratic curve.
    /// </summary>
    private static SKPath CreateRoundedPolygon(IReadOnlyList<SKPoint> points, float radius)
    {
        SKPath path = new SKPath();
        if (points.Count == 0)
        {
            return path;
        }

        if (radius <= 0)
        {
#pragma warning disable CS0618
            path.AddPoly(points.ToArray(), true);
#pragma warning restore CS0618
            return path;
        }

        for (int i = 0; i < points.Count; i++)
        {
            SKPoint previous = points[(i - 1 + points.Count) % points.Count];
            SKPoint current = points[i];
            SKPoint next = points[(i + 1) % points.Count];
            float distanceToPrevious = Distance(current, previous);
            float distanceToNext = Distance(current, next);
            float cornerRadius = MathF.Min(radius, MathF.Min(distanceToPrevious, distanceToNext) * 0.45f);

            SKPoint start = MoveTowards(current, previous, cornerRadius);
            SKPoint end = MoveTowards(current, next, cornerRadius);

            if (i == 0)
            {
#pragma warning disable CS0618
                path.MoveTo(start);
#pragma warning restore CS0618
            }
            else
            {
#pragma warning disable CS0618
                path.LineTo(start);
#pragma warning restore CS0618
            }

#pragma warning disable CS0618
            path.QuadTo(current, end);
#pragma warning restore CS0618
        }

#pragma warning disable CS0618
        path.Close();
#pragma warning restore CS0618
        return path;
    }

    /// <summary>
    /// Creates the blurred paint used to draw the active segment glow.
    /// </summary>
    private SKPaint? CreateGlowPaint()
    {
        if (!IsGlowEnabled || GlowIntensity <= 0)
        {
            return null;
        }

        // The glow is just the same active shape drawn first with a blurred
        // mask. Drawing the filled segment after it keeps the LED face crisp.
        return new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = ToSkColor(GlowColor, Clamp01((float)GlowIntensity)),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, MathF.Max(2, (float)SegmentThickness * 0.55f))
        };
    }

    /// <summary>
    /// Creates an anti-aliased fill paint for crisp segment faces.
    /// </summary>
    private static SKPaint CreateFillPaint(SKColor color) =>
        new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = color
        };

    /// <summary>
    /// Measures the string in design-space units before it is scaled to the canvas.
    /// </summary>
    private static float MeasureText(string text, float characterWidth, float symbolWidth, float spacing)
    {
        if (text.Length == 0)
        {
            return 0;
        }

        float width = 0f;
        foreach (char character in text)
        {
            width += IsSymbol(character) ? symbolWidth : characterWidth;
        }

        return width + spacing * (text.Length - 1);
    }

    /// <summary>
    /// Returns true for punctuation symbols that use a narrower slot than normal characters.
    /// </summary>
    private static bool IsSymbol(char character) => character is ':' or '.';

    /// <summary>
    /// Calculates the top-left drawing origin after applying horizontal and vertical alignment.
    /// </summary>
    private SKPoint GetAlignedOrigin(float canvasWidth, float canvasHeight, float contentWidth, float contentHeight)
    {
        float x = HorizontalTextAlignment switch
        {
            TextAlignment.Start => 12f,
            TextAlignment.End => canvasWidth - contentWidth - 12f,
            _ => (canvasWidth - contentWidth) / 2f
        };

        float y = VerticalTextAlignment switch
        {
            TextAlignment.Start => 10f,
            TextAlignment.End => canvasHeight - contentHeight - 10f,
            _ => (canvasHeight - contentHeight) / 2f
        };

        return new SKPoint(MathF.Max(0, x), MathF.Max(0, y));
    }

    /// <summary>
    /// Moves from one point toward another by a fixed distance.
    /// </summary>
    private static SKPoint MoveTowards(SKPoint from, SKPoint to, float distance)
    {
        float totalDistance = Distance(from, to);
        if (totalDistance <= 0)
        {
            return from;
        }

        float ratio = distance / totalDistance;
        return new SKPoint(
            from.X + (to.X - from.X) * ratio,
            from.Y + (to.Y - from.Y) * ratio);
    }

    /// <summary>
    /// Calculates the Euclidean distance between two Skia points.
    /// </summary>
    private static float Distance(SKPoint a, SKPoint b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Converts a MAUI color to an Skia color while applying an extra alpha multiplier.
    /// </summary>
    private static SKColor ToSkColor(Color color, float alphaMultiplier = 1f)
    {
        float alpha = Clamp01(color.Alpha * alphaMultiplier);
        return new SKColor(
            (byte)MathF.Round(Clamp01(color.Red) * 255),
            (byte)MathF.Round(Clamp01(color.Green) * 255),
            (byte)MathF.Round(Clamp01(color.Blue) * 255),
            (byte)MathF.Round(alpha * 255));
    }

    /// <summary>
    /// Returns an existing Skia color with a replaced alpha channel.
    /// </summary>
    private static SKColor WithAlpha(SKColor color, float alpha) 
        => new(color.Red, color.Green, color.Blue, (byte)MathF.Round(Clamp01(alpha) * 255));

    /// <summary>
    /// Clamps a floating-point value to the 0..1 range used by alpha and animation progress.
    /// </summary>
    private static float Clamp01(float value) 
        => Math.Clamp(value, 0f, 1f);
}
