using CoaSTL.Core.Models;

namespace CoaSTL.Core.Geometry;

/// <summary>
/// Generates 3D text meshes for embossing/debossing on coasters.
/// Uses a simplified bitmap-based approach for text rendering.
/// </summary>
public static class TextGenerator
{
    // Simplified 5x7 bitmap font for basic characters
    private static readonly Dictionary<char, bool[,]> Font = CreateSimpleFont();

    /// <summary>
    /// Generates triangles for embossed/debossed text.
    /// </summary>
    public static List<Triangle> GenerateText(
        TextElement textElement,
        float coasterDiameter,
        float baseHeight)
    {
        var triangles = new List<Triangle>();
        var text = textElement.Text.ToUpperInvariant();

        if (string.IsNullOrEmpty(text))
            return triangles;

        // Calculate character dimensions
        var charHeight = textElement.FontSize;
        var charWidth = charHeight * 5f / 7f; // 5:7 aspect ratio for our bitmap font
        var spacing = charWidth * textElement.LetterSpacing * 0.2f;

        // Total text width
        var totalWidth = text.Length * charWidth + (text.Length - 1) * spacing;

        // Starting position based on alignment
        float startX, startY;

        switch (textElement.Alignment)
        {
            case TextAlignment.TopCenter:
                startX = -totalWidth / 2 + textElement.OffsetX;
                startY = coasterDiameter * 0.25f + textElement.OffsetY;
                break;
            case TextAlignment.BottomCenter:
                startX = -totalWidth / 2 + textElement.OffsetX;
                startY = -coasterDiameter * 0.25f - charHeight + textElement.OffsetY;
                break;
            case TextAlignment.Center:
            default:
                startX = -totalWidth / 2 + textElement.OffsetX;
                startY = -charHeight / 2 + textElement.OffsetY;
                break;
        }

        // Calculate Z positions
        var topZ = textElement.Embossed
            ? baseHeight + textElement.Depth
            : baseHeight;
        var bottomZ = textElement.Embossed
            ? baseHeight
            : baseHeight - textElement.Depth;

        // Generate each character
        float currentX = startX;
        foreach (var c in text)
        {
            if (Font.TryGetValue(c, out var bitmap))
            {
                GenerateCharacter(triangles, bitmap, currentX, startY, charWidth, charHeight,
                    topZ, bottomZ, textElement.Embossed, textElement.Rotation);
            }
            else if (c == ' ')
            {
                // Space - just advance
            }
            else
            {
                // Unknown character - render as block
                GenerateBlock(triangles, currentX, startY, charWidth, charHeight, topZ, bottomZ);
            }

            currentX += charWidth + spacing;
        }

        return triangles;
    }

    private static void GenerateCharacter(
        List<Triangle> triangles,
        bool[,] bitmap,
        float startX,
        float startY,
        float charWidth,
        float charHeight,
        float topZ,
        float bottomZ,
        bool embossed,
        float rotation)
    {
        var pixelWidth = charWidth / 5;
        var pixelHeight = charHeight / 7;

        for (int row = 0; row < 7; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                if (bitmap[row, col])
                {
                    var x = startX + col * pixelWidth;
                    var y = startY + (6 - row) * pixelHeight; // Flip Y for proper orientation

                    // Apply rotation if specified
                    if (MathF.Abs(rotation) > 0.001f)
                    {
                        var rad = rotation * MathF.PI / 180f;
                        var cos = MathF.Cos(rad);
                        var sin = MathF.Sin(rad);
                        var newX = x * cos - y * sin;
                        var newY = x * sin + y * cos;
                        x = newX;
                        y = newY;
                    }

                    GeneratePixelBlock(triangles, x, y, pixelWidth, pixelHeight, topZ, bottomZ);
                }
            }
        }
    }

    private static void GeneratePixelBlock(
        List<Triangle> triangles,
        float x,
        float y,
        float width,
        float height,
        float topZ,
        float bottomZ)
    {
        // Create a small extruded block for each "pixel" of the character

        var v1Top = new Vector3(x, y, topZ);
        var v2Top = new Vector3(x + width, y, topZ);
        var v3Top = new Vector3(x + width, y + height, topZ);
        var v4Top = new Vector3(x, y + height, topZ);

        var v1Bot = new Vector3(x, y, bottomZ);
        var v2Bot = new Vector3(x + width, y, bottomZ);
        var v3Bot = new Vector3(x + width, y + height, bottomZ);
        var v4Bot = new Vector3(x, y + height, bottomZ);

        // Top face
        triangles.Add(new Triangle(v1Top, v2Top, v3Top));
        triangles.Add(new Triangle(v1Top, v3Top, v4Top));

        // Bottom face
        triangles.Add(new Triangle(v1Bot, v3Bot, v2Bot));
        triangles.Add(new Triangle(v1Bot, v4Bot, v3Bot));

        // Side faces
        triangles.Add(new Triangle(v1Top, v1Bot, v2Bot));
        triangles.Add(new Triangle(v1Top, v2Bot, v2Top));

        triangles.Add(new Triangle(v2Top, v2Bot, v3Bot));
        triangles.Add(new Triangle(v2Top, v3Bot, v3Top));

        triangles.Add(new Triangle(v3Top, v3Bot, v4Bot));
        triangles.Add(new Triangle(v3Top, v4Bot, v4Top));

        triangles.Add(new Triangle(v4Top, v4Bot, v1Bot));
        triangles.Add(new Triangle(v4Top, v1Bot, v1Top));
    }

    private static void GenerateBlock(
        List<Triangle> triangles,
        float x,
        float y,
        float width,
        float height,
        float topZ,
        float bottomZ)
    {
        GeneratePixelBlock(triangles, x, y, width, height, topZ, bottomZ);
    }

    private static Dictionary<char, bool[,]> CreateSimpleFont()
    {
        // Simple 5x7 bitmap font
        var font = new Dictionary<char, bool[,]>
        {
            ['A'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true }
            },
            ['B'] = new bool[,]
            {
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false }
            },
            ['C'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['D'] = new bool[,]
            {
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false }
            },
            ['E'] = new bool[,]
            {
                { true, true, true, true, true },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, true }
            },
            ['F'] = new bool[,]
            {
                { true, true, true, true, true },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false }
            },
            ['G'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, false },
                { true, false, true, true, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['H'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true }
            },
            ['I'] = new bool[,]
            {
                { true, true, true, true, true },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { true, true, true, true, true }
            },
            ['J'] = new bool[,]
            {
                { false, false, false, false, true },
                { false, false, false, false, true },
                { false, false, false, false, true },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['K'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, true, false },
                { true, false, true, false, false },
                { true, true, false, false, false },
                { true, false, true, false, false },
                { true, false, false, true, false },
                { true, false, false, false, true }
            },
            ['L'] = new bool[,]
            {
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, true }
            },
            ['M'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, true, false, true, true },
                { true, false, true, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true }
            },
            ['N'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, true, false, false, true },
                { true, false, true, false, true },
                { true, false, false, true, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true }
            },
            ['O'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['P'] = new bool[,]
            {
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false }
            },
            ['Q'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, true, false, true },
                { true, false, false, true, false },
                { false, true, true, false, true }
            },
            ['R'] = new bool[,]
            {
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false },
                { true, false, true, false, false },
                { true, false, false, true, false },
                { true, false, false, false, true }
            },
            ['S'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, false },
                { false, true, true, true, false },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['T'] = new bool[,]
            {
                { true, true, true, true, true },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false }
            },
            ['U'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['V'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, false, true, false },
                { false, true, false, true, false },
                { false, false, true, false, false }
            },
            ['W'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, true, false, true },
                { true, false, true, false, true },
                { true, true, false, true, true },
                { true, false, false, false, true }
            },
            ['X'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, false, true, false },
                { false, false, true, false, false },
                { false, true, false, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true }
            },
            ['Y'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, false, true, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false }
            },
            ['Z'] = new bool[,]
            {
                { true, true, true, true, true },
                { false, false, false, false, true },
                { false, false, false, true, false },
                { false, false, true, false, false },
                { false, true, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, true }
            },
            ['0'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, true, true },
                { true, false, true, false, true },
                { true, true, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['1'] = new bool[,]
            {
                { false, false, true, false, false },
                { false, true, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, true, true, true, false }
            },
            ['2'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { false, false, false, false, true },
                { false, false, true, true, false },
                { false, true, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, true }
            },
            ['3'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { false, false, false, false, true },
                { false, false, true, true, false },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['4'] = new bool[,]
            {
                { false, false, false, true, false },
                { false, false, true, true, false },
                { false, true, false, true, false },
                { true, false, false, true, false },
                { true, true, true, true, true },
                { false, false, false, true, false },
                { false, false, false, true, false }
            },
            ['5'] = new bool[,]
            {
                { true, true, true, true, true },
                { true, false, false, false, false },
                { true, true, true, true, false },
                { false, false, false, false, true },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['6'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['7'] = new bool[,]
            {
                { true, true, true, true, true },
                { false, false, false, false, true },
                { false, false, false, true, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false }
            },
            ['8'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            },
            ['9'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, true },
                { false, false, false, false, true },
                { false, false, false, false, true },
                { false, true, true, true, false }
            },
            ['-'] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { true, true, true, true, true },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false }
            },
            ['.'] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false }
            },
            ['!'] = new bool[,]
            {
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false }
            }
        };

        return font;
    }
}
