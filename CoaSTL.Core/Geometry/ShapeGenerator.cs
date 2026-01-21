using CoaSTL.Core.Models;

namespace CoaSTL.Core.Geometry;

/// <summary>
/// Generates 2D profile points for various coaster shapes.
/// </summary>
public static class ShapeGenerator
{
    /// <summary>
    /// Generates a circle profile as a list of 2D points.
    /// </summary>
    public static List<(float X, float Y)> GenerateCircle(float radius, int segments)
    {
        var points = new List<(float X, float Y)>();
        for (int i = 0; i < segments; i++)
        {
            var angle = 2 * MathF.PI * i / segments;
            points.Add((radius * MathF.Cos(angle), radius * MathF.Sin(angle)));
        }
        return points;
    }

    /// <summary>
    /// Generates a square profile as a list of 2D points.
    /// </summary>
    public static List<(float X, float Y)> GenerateSquare(float size)
    {
        var half = size / 2;
        return new List<(float X, float Y)>
        {
            (-half, -half),
            (half, -half),
            (half, half),
            (-half, half)
        };
    }

    /// <summary>
    /// Generates a rounded square profile.
    /// </summary>
    public static List<(float X, float Y)> GenerateRoundedSquare(float size, float cornerRadius, int segmentsPerCorner)
    {
        var points = new List<(float X, float Y)>();
        var half = size / 2;
        var r = Math.Min(cornerRadius, half - 1);

        // Generate points for each corner arc
        var corners = new[]
        {
            (half - r, half - r, 0f),      // Top-right
            (-half + r, half - r, MathF.PI / 2),   // Top-left
            (-half + r, -half + r, MathF.PI),      // Bottom-left
            (half - r, -half + r, 3 * MathF.PI / 2) // Bottom-right
        };

        foreach (var (cx, cy, startAngle) in corners)
        {
            for (int i = 0; i <= segmentsPerCorner; i++)
            {
                var angle = startAngle + (MathF.PI / 2) * i / segmentsPerCorner;
                points.Add((cx + r * MathF.Cos(angle), cy + r * MathF.Sin(angle)));
            }
        }

        return points;
    }

    /// <summary>
    /// Generates a regular polygon profile.
    /// </summary>
    public static List<(float X, float Y)> GeneratePolygon(float radius, int sides)
    {
        var points = new List<(float X, float Y)>();
        var angleOffset = -MathF.PI / 2; // Start from top

        for (int i = 0; i < sides; i++)
        {
            var angle = angleOffset + 2 * MathF.PI * i / sides;
            points.Add((radius * MathF.Cos(angle), radius * MathF.Sin(angle)));
        }

        return points;
    }

    /// <summary>
    /// Generates a hexagon profile.
    /// </summary>
    public static List<(float X, float Y)> GenerateHexagon(float radius) => GeneratePolygon(radius, 6);

    /// <summary>
    /// Generates an octagon profile.
    /// </summary>
    public static List<(float X, float Y)> GenerateOctagon(float radius) => GeneratePolygon(radius, 8);

    /// <summary>
    /// Generates the profile based on coaster settings.
    /// </summary>
    public static List<(float X, float Y)> GenerateProfile(CoasterSettings settings)
    {
        var radius = settings.Diameter / 2;
        var segments = settings.CurveResolution * 4;

        return settings.Shape switch
        {
            CoasterShape.Circle => GenerateCircle(radius, segments),
            CoasterShape.Square => GenerateSquare(settings.Diameter),
            CoasterShape.Hexagon => GenerateHexagon(radius),
            CoasterShape.Octagon => GenerateOctagon(radius),
            CoasterShape.RoundedSquare => GenerateRoundedSquare(settings.Diameter, settings.CornerRadius, settings.CurveResolution),
            CoasterShape.CustomPolygon => GeneratePolygon(radius, settings.PolygonSides),
            _ => GenerateCircle(radius, segments)
        };
    }
}
