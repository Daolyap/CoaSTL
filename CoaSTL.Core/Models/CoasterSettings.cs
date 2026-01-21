namespace CoaSTL.Core.Models;

/// <summary>
/// Defines the available coaster shapes.
/// </summary>
public enum CoasterShape
{
    Circle,
    Square,
    Hexagon,
    Octagon,
    RoundedSquare,
    CustomPolygon
}

/// <summary>
/// Defines edge styles for coasters.
/// </summary>
public enum EdgeStyle
{
    Flat,
    Beveled,
    Rounded,
    RaisedRim
}

/// <summary>
/// Configuration settings for a coaster design.
/// </summary>
public sealed class CoasterSettings
{
    /// <summary>
    /// The shape of the coaster.
    /// </summary>
    public CoasterShape Shape { get; set; } = CoasterShape.Circle;

    /// <summary>
    /// Diameter or width of the coaster in millimeters (70-150mm).
    /// </summary>
    public float Diameter { get; set; } = 100f;

    /// <summary>
    /// Base thickness in millimeters (2-8mm).
    /// </summary>
    public float BaseThickness { get; set; } = 4f;

    /// <summary>
    /// Total height including features in millimeters (3-15mm).
    /// </summary>
    public float TotalHeight { get; set; } = 6f;

    /// <summary>
    /// Edge style of the coaster.
    /// </summary>
    public EdgeStyle EdgeStyle { get; set; } = EdgeStyle.Flat;

    /// <summary>
    /// Bevel angle in degrees (for beveled edges).
    /// </summary>
    public float BevelAngle { get; set; } = 45f;

    /// <summary>
    /// Corner radius for rounded square shape in millimeters.
    /// </summary>
    public float CornerRadius { get; set; } = 10f;

    /// <summary>
    /// Number of sides for custom polygon shape (3-12).
    /// </summary>
    public int PolygonSides { get; set; } = 6;

    /// <summary>
    /// Relief depth in millimeters (0.5-5mm).
    /// </summary>
    public float ReliefDepth { get; set; } = 1.5f;

    /// <summary>
    /// Whether to invert the relief (embossed vs debossed).
    /// </summary>
    public bool InvertRelief { get; set; }

    /// <summary>
    /// Whether to add non-slip pattern on the bottom.
    /// </summary>
    public bool AddNonSlipBottom { get; set; }

    /// <summary>
    /// Resolution for curve approximation (segments per 90 degrees).
    /// </summary>
    public int CurveResolution { get; set; } = 16;

    /// <summary>
    /// Validates the settings and clamps values to acceptable ranges.
    /// </summary>
    public void Validate()
    {
        Diameter = Math.Clamp(Diameter, 70f, 150f);
        BaseThickness = Math.Clamp(BaseThickness, 2f, 8f);
        TotalHeight = Math.Clamp(TotalHeight, 3f, 15f);
        CornerRadius = Math.Clamp(CornerRadius, 1f, Diameter / 4f);
        PolygonSides = Math.Clamp(PolygonSides, 3, 12);
        ReliefDepth = Math.Clamp(ReliefDepth, 0.5f, 5f);
        BevelAngle = Math.Clamp(BevelAngle, 15f, 75f);
        CurveResolution = Math.Clamp(CurveResolution, 8, 64);

        // Ensure total height is at least base thickness plus minimum relief
        if (TotalHeight < BaseThickness + 0.5f)
        {
            TotalHeight = BaseThickness + 0.5f;
        }
    }
}
