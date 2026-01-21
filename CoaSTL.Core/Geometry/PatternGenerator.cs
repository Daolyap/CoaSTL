using CoaSTL.Core.Models;

namespace CoaSTL.Core.Geometry;

/// <summary>
/// Generates base patterns for coaster bottoms.
/// </summary>
public static class PatternGenerator
{
    /// <summary>
    /// Generates a honeycomb pattern within the given profile.
    /// </summary>
    public static List<Triangle> GenerateHoneycombPattern(
        List<(float X, float Y)> profile,
        float cellSize,
        float depth,
        float baseZ)
    {
        var triangles = new List<Triangle>();
        var (minX, maxX, minY, maxY) = GetBounds(profile);

        // Generate hexagonal grid
        var hexRadius = cellSize / 2;
        var hexHeight = hexRadius * MathF.Sqrt(3);
        var hexWidth = hexRadius * 2;

        for (float y = minY; y <= maxY; y += hexHeight * 1.5f)
        {
            bool offset = ((int)((y - minY) / (hexHeight * 1.5f))) % 2 == 1;
            float startX = offset ? minX + hexWidth * 0.75f : minX;

            for (float x = startX; x <= maxX; x += hexWidth * 1.5f)
            {
                if (IsPointInProfile(x, y, profile))
                {
                    AddHexagonHole(triangles, x, y, hexRadius * 0.8f, depth, baseZ);
                }
            }
        }

        return triangles;
    }

    /// <summary>
    /// Generates a grid pattern within the given profile.
    /// </summary>
    public static List<Triangle> GenerateGridPattern(
        List<(float X, float Y)> profile,
        float spacing,
        float lineWidth,
        float depth,
        float baseZ)
    {
        var triangles = new List<Triangle>();
        var (minX, maxX, minY, maxY) = GetBounds(profile);

        // Horizontal lines
        for (float y = minY + spacing; y < maxY; y += spacing)
        {
            for (float x = minX; x < maxX - lineWidth; x += lineWidth * 2)
            {
                if (IsPointInProfile(x, y, profile) && IsPointInProfile(x + lineWidth, y, profile))
                {
                    AddGroove(triangles, x, y, x + lineWidth, y, lineWidth, depth, baseZ);
                }
            }
        }

        // Vertical lines
        for (float x = minX + spacing; x < maxX; x += spacing)
        {
            for (float y = minY; y < maxY - lineWidth; y += lineWidth * 2)
            {
                if (IsPointInProfile(x, y, profile) && IsPointInProfile(x, y + lineWidth, profile))
                {
                    AddGroove(triangles, x, y, x, y + lineWidth, lineWidth, depth, baseZ);
                }
            }
        }

        return triangles;
    }

    /// <summary>
    /// Generates concentric circle pattern within the given profile.
    /// </summary>
    public static List<Triangle> GenerateConcentricCircles(
        List<(float X, float Y)> profile,
        float spacing,
        float grooveWidth,
        float depth,
        float baseZ)
    {
        var triangles = new List<Triangle>();
        var (minX, maxX, minY, maxY) = GetBounds(profile);
        var centerX = (minX + maxX) / 2;
        var centerY = (minY + maxY) / 2;
        var maxRadius = Math.Min(maxX - centerX, maxY - centerY);

        for (float radius = spacing; radius < maxRadius; radius += spacing)
        {
            AddCircularGroove(triangles, centerX, centerY, radius, grooveWidth, depth, baseZ, 32);
        }

        return triangles;
    }

    /// <summary>
    /// Generates drainage grooves on the top surface.
    /// </summary>
    public static List<Triangle> GenerateDrainageGrooves(
        List<(float X, float Y)> profile,
        int grooveCount,
        float grooveWidth,
        float grooveDepth,
        float topZ)
    {
        var triangles = new List<Triangle>();
        var (minX, maxX, minY, maxY) = GetBounds(profile);
        var centerX = (minX + maxX) / 2;
        var centerY = (minY + maxY) / 2;
        var maxRadius = Math.Min(maxX - centerX, maxY - centerY) * 0.8f;

        for (int i = 0; i < grooveCount; i++)
        {
            var angle = 2 * MathF.PI * i / grooveCount;
            var endX = centerX + maxRadius * MathF.Cos(angle);
            var endY = centerY + maxRadius * MathF.Sin(angle);

            AddGroove(triangles, centerX, centerY, endX, endY, grooveWidth, grooveDepth, topZ);
        }

        return triangles;
    }

    /// <summary>
    /// Generates non-slip dot pattern on the bottom.
    /// </summary>
    public static List<Triangle> GenerateNonSlipDots(
        List<(float X, float Y)> profile,
        float dotRadius,
        float dotHeight,
        float spacing)
    {
        var triangles = new List<Triangle>();
        var (minX, maxX, minY, maxY) = GetBounds(profile);

        for (float x = minX + spacing; x < maxX - spacing; x += spacing)
        {
            for (float y = minY + spacing; y < maxY - spacing; y += spacing)
            {
                if (IsPointInProfile(x, y, profile))
                {
                    AddDot(triangles, x, y, dotRadius, dotHeight);
                }
            }
        }

        return triangles;
    }

    private static void AddHexagonHole(List<Triangle> triangles, float cx, float cy, float radius, float depth, float baseZ)
    {
        const int sides = 6;
        var topZ = baseZ;
        var bottomZ = baseZ - depth;

        var points = new (float X, float Y)[sides];
        for (int i = 0; i < sides; i++)
        {
            var angle = 2 * MathF.PI * i / sides;
            points[i] = (cx + radius * MathF.Cos(angle), cy + radius * MathF.Sin(angle));
        }

        // Bottom face
        var center = new Vector3(cx, cy, bottomZ);
        for (int i = 0; i < sides; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % sides];
            triangles.Add(new Triangle(
                center,
                new Vector3(p2.X, p2.Y, bottomZ),
                new Vector3(p1.X, p1.Y, bottomZ)
            ));
        }

        // Side walls
        for (int i = 0; i < sides; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % sides];

            triangles.Add(new Triangle(
                new Vector3(p1.X, p1.Y, topZ),
                new Vector3(p2.X, p2.Y, topZ),
                new Vector3(p2.X, p2.Y, bottomZ)
            ));
            triangles.Add(new Triangle(
                new Vector3(p1.X, p1.Y, topZ),
                new Vector3(p2.X, p2.Y, bottomZ),
                new Vector3(p1.X, p1.Y, bottomZ)
            ));
        }
    }

    private static void AddGroove(List<Triangle> triangles, float x1, float y1, float x2, float y2,
        float width, float depth, float topZ)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        var len = MathF.Sqrt(dx * dx + dy * dy);
        if (len < 0.001f) return;

        // Perpendicular direction
        var px = -dy / len * width / 2;
        var py = dx / len * width / 2;

        var bottomZ = topZ - depth;

        // Create groove as a rectangular channel
        var v1Top = new Vector3(x1 + px, y1 + py, topZ);
        var v2Top = new Vector3(x1 - px, y1 - py, topZ);
        var v3Top = new Vector3(x2 - px, y2 - py, topZ);
        var v4Top = new Vector3(x2 + px, y2 + py, topZ);

        var v1Bot = new Vector3(x1 + px, y1 + py, bottomZ);
        var v2Bot = new Vector3(x1 - px, y1 - py, bottomZ);
        var v3Bot = new Vector3(x2 - px, y2 - py, bottomZ);
        var v4Bot = new Vector3(x2 + px, y2 + py, bottomZ);

        // Bottom face
        triangles.Add(new Triangle(v1Bot, v3Bot, v2Bot));
        triangles.Add(new Triangle(v1Bot, v4Bot, v3Bot));

        // Side walls
        triangles.Add(new Triangle(v1Top, v2Top, v2Bot));
        triangles.Add(new Triangle(v1Top, v2Bot, v1Bot));

        triangles.Add(new Triangle(v2Top, v3Top, v3Bot));
        triangles.Add(new Triangle(v2Top, v3Bot, v2Bot));

        triangles.Add(new Triangle(v3Top, v4Top, v4Bot));
        triangles.Add(new Triangle(v3Top, v4Bot, v3Bot));

        triangles.Add(new Triangle(v4Top, v1Top, v1Bot));
        triangles.Add(new Triangle(v4Top, v1Bot, v4Bot));
    }

    private static void AddCircularGroove(List<Triangle> triangles, float cx, float cy,
        float radius, float width, float depth, float topZ, int segments)
    {
        var bottomZ = topZ - depth;
        var innerRadius = radius - width / 2;
        var outerRadius = radius + width / 2;

        for (int i = 0; i < segments; i++)
        {
            var angle1 = 2 * MathF.PI * i / segments;
            var angle2 = 2 * MathF.PI * (i + 1) / segments;

            var cos1 = MathF.Cos(angle1);
            var sin1 = MathF.Sin(angle1);
            var cos2 = MathF.Cos(angle2);
            var sin2 = MathF.Sin(angle2);

            // Inner edge at angle1 and angle2
            var in1 = new Vector3(cx + innerRadius * cos1, cy + innerRadius * sin1, topZ);
            var in2 = new Vector3(cx + innerRadius * cos2, cy + innerRadius * sin2, topZ);
            var in1b = new Vector3(cx + innerRadius * cos1, cy + innerRadius * sin1, bottomZ);
            var in2b = new Vector3(cx + innerRadius * cos2, cy + innerRadius * sin2, bottomZ);

            // Outer edge
            var out1 = new Vector3(cx + outerRadius * cos1, cy + outerRadius * sin1, topZ);
            var out2 = new Vector3(cx + outerRadius * cos2, cy + outerRadius * sin2, topZ);
            var out1b = new Vector3(cx + outerRadius * cos1, cy + outerRadius * sin1, bottomZ);
            var out2b = new Vector3(cx + outerRadius * cos2, cy + outerRadius * sin2, bottomZ);

            // Bottom face
            triangles.Add(new Triangle(in1b, in2b, out2b));
            triangles.Add(new Triangle(in1b, out2b, out1b));

            // Inner wall
            triangles.Add(new Triangle(in1, in1b, in2b));
            triangles.Add(new Triangle(in1, in2b, in2));

            // Outer wall
            triangles.Add(new Triangle(out1, out2b, out1b));
            triangles.Add(new Triangle(out1, out2, out2b));
        }
    }

    private static void AddDot(List<Triangle> triangles, float x, float y, float radius, float height)
    {
        const int segments = 8;
        var bottom = new Vector3(x, y, 0);
        var top = new Vector3(x, y, -height);

        for (int i = 0; i < segments; i++)
        {
            var angle1 = 2 * MathF.PI * i / segments;
            var angle2 = 2 * MathF.PI * (i + 1) / segments;

            var p1 = new Vector3(x + radius * MathF.Cos(angle1), y + radius * MathF.Sin(angle1), 0);
            var p2 = new Vector3(x + radius * MathF.Cos(angle2), y + radius * MathF.Sin(angle2), 0);

            // Side face
            triangles.Add(new Triangle(bottom, p1, p2));

            // Bottom cap
            triangles.Add(new Triangle(top, p2, p1));
        }
    }

    private static (float minX, float maxX, float minY, float maxY) GetBounds(List<(float X, float Y)> profile)
    {
        var minX = profile.Min(p => p.X);
        var maxX = profile.Max(p => p.X);
        var minY = profile.Min(p => p.Y);
        var maxY = profile.Max(p => p.Y);
        return (minX, maxX, minY, maxY);
    }

    private static bool IsPointInProfile(float x, float y, List<(float X, float Y)> profile)
    {
        int n = profile.Count;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var pi = profile[i];
            var pj = profile[j];

            if ((pi.Y > y) != (pj.Y > y) &&
                x < (pj.X - pi.X) * (y - pi.Y) / (pj.Y - pi.Y) + pi.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
