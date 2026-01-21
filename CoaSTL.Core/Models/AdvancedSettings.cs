namespace CoaSTL.Core.Models;

/// <summary>
/// Base pattern types for coaster bottoms.
/// </summary>
public enum BasePatternType
{
    Solid,
    Honeycomb,
    Grid,
    ConcentricCircles,
    Voronoi,
    Dots
}

/// <summary>
/// Surface pattern types for coaster tops.
/// </summary>
public enum SurfacePatternType
{
    None,
    DrainageGrooves,
    Ripples,
    Waves,
    Pyramids,
    Bubbles
}

/// <summary>
/// Text alignment options.
/// </summary>
public enum TextAlignment
{
    Center,
    TopCenter,
    BottomCenter,
    Curved,
    Circular
}

/// <summary>
/// Represents text to be embossed or debossed on the coaster.
/// </summary>
public sealed class TextElement
{
    /// <summary>
    /// The text content.
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// Font size in millimeters (height of characters).
    /// </summary>
    public float FontSize { get; set; } = 8f;

    /// <summary>
    /// Depth of the text in millimeters.
    /// </summary>
    public float Depth { get; set; } = 1f;

    /// <summary>
    /// Whether the text is embossed (raised) or debossed (recessed).
    /// </summary>
    public bool Embossed { get; set; } = true;

    /// <summary>
    /// Text alignment on the coaster surface.
    /// </summary>
    public TextAlignment Alignment { get; set; } = TextAlignment.Center;

    /// <summary>
    /// X offset from center in millimeters.
    /// </summary>
    public float OffsetX { get; set; }

    /// <summary>
    /// Y offset from center in millimeters.
    /// </summary>
    public float OffsetY { get; set; }

    /// <summary>
    /// Rotation angle in degrees.
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// Font name (system font or built-in).
    /// </summary>
    public string FontName { get; set; } = "Arial";

    /// <summary>
    /// Whether to use bold font.
    /// </summary>
    public bool Bold { get; set; }

    /// <summary>
    /// Letter spacing multiplier (1.0 = normal).
    /// </summary>
    public float LetterSpacing { get; set; } = 1.0f;
}

/// <summary>
/// Extended coaster settings with advanced features.
/// </summary>
public sealed class AdvancedCoasterSettings
{
    /// <summary>
    /// Base pattern type.
    /// </summary>
    public BasePatternType BasePattern { get; set; } = BasePatternType.Solid;

    /// <summary>
    /// Surface pattern type.
    /// </summary>
    public SurfacePatternType SurfacePattern { get; set; } = SurfacePatternType.None;

    /// <summary>
    /// Text elements to add to the coaster.
    /// </summary>
    public List<TextElement> TextElements { get; set; } = new();

    /// <summary>
    /// Enable lithophane mode.
    /// </summary>
    public bool LithophaneMode { get; set; }

    /// <summary>
    /// Minimum thickness for lithophane (mm).
    /// </summary>
    public float LithophaneMinThickness { get; set; } = 0.8f;

    /// <summary>
    /// Maximum thickness for lithophane (mm).
    /// </summary>
    public float LithophaneMaxThickness { get; set; } = 3.0f;

    /// <summary>
    /// Add stacking features (nubs for stacking coasters).
    /// </summary>
    public bool AddStackingFeatures { get; set; }

    /// <summary>
    /// Add drainage grooves for wet glasses.
    /// </summary>
    public bool AddDrainageGrooves { get; set; }

    /// <summary>
    /// Number of drainage grooves.
    /// </summary>
    public int DrainageGrooveCount { get; set; } = 3;

    /// <summary>
    /// Drainage groove depth in mm.
    /// </summary>
    public float DrainageGrooveDepth { get; set; } = 0.5f;

    /// <summary>
    /// Cork/rubber insert recess depth (0 = none).
    /// </summary>
    public float InsertRecessDepth { get; set; }

    /// <summary>
    /// Magnet recess depth and diameter (0 = none).
    /// </summary>
    public float MagnetRecessDepth { get; set; }
    public float MagnetDiameter { get; set; } = 6f;

    /// <summary>
    /// Number of magnets to add.
    /// </summary>
    public int MagnetCount { get; set; } = 3;

    /// <summary>
    /// Honeycomb cell size for honeycomb base pattern.
    /// </summary>
    public float HoneycombCellSize { get; set; } = 5f;

    /// <summary>
    /// Grid spacing for grid base pattern.
    /// </summary>
    public float GridSpacing { get; set; } = 5f;

    /// <summary>
    /// Rim width for raised rim edge style.
    /// </summary>
    public float RimWidth { get; set; } = 3f;

    /// <summary>
    /// Rim height for raised rim edge style.
    /// </summary>
    public float RimHeight { get; set; } = 2f;
}
