using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoaSTL.Core.Models;

/// <summary>
/// Coaster design template for saving and loading projects.
/// </summary>
public sealed class CoasterTemplate
{
    /// <summary>
    /// Template format version.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Template name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Template description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Author name.
    /// </summary>
    public string Author { get; set; } = "";

    /// <summary>
    /// Creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modified date.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Template tags for categorization.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Basic coaster settings.
    /// </summary>
    public CoasterSettings Settings { get; set; } = new();

    /// <summary>
    /// Advanced coaster settings.
    /// </summary>
    public AdvancedCoasterSettings AdvancedSettings { get; set; } = new();

    /// <summary>
    /// Image file path (relative or absolute).
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Base64-encoded image data (for embedded images).
    /// </summary>
    public string? ImageData { get; set; }

    /// <summary>
    /// Material recommendation.
    /// </summary>
    public MaterialType RecommendedMaterial { get; set; } = MaterialType.PLA;

    /// <summary>
    /// Preview thumbnail as base64.
    /// </summary>
    public string? ThumbnailBase64 { get; set; }
}

/// <summary>
/// Manages saving and loading coaster templates.
/// </summary>
public static class TemplateManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Saves a template to a JSON file.
    /// </summary>
    public static void SaveTemplate(CoasterTemplate template, string filePath)
    {
        template.ModifiedAt = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(template, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads a template from a JSON file.
    /// </summary>
    public static CoasterTemplate LoadTemplate(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Template file not found.", filePath);

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<CoasterTemplate>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize template.");
    }

    /// <summary>
    /// Saves a template to a JSON string.
    /// </summary>
    public static string SerializeTemplate(CoasterTemplate template)
    {
        return JsonSerializer.Serialize(template, JsonOptions);
    }

    /// <summary>
    /// Loads a template from a JSON string.
    /// </summary>
    public static CoasterTemplate DeserializeTemplate(string json)
    {
        return JsonSerializer.Deserialize<CoasterTemplate>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize template.");
    }

    /// <summary>
    /// Creates a template from current settings.
    /// </summary>
    public static CoasterTemplate CreateTemplate(
        string name,
        CoasterSettings settings,
        AdvancedCoasterSettings? advancedSettings = null,
        string? imagePath = null)
    {
        return new CoasterTemplate
        {
            Name = name,
            Settings = settings,
            AdvancedSettings = advancedSettings ?? new AdvancedCoasterSettings(),
            ImagePath = imagePath,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Validates a template.
    /// </summary>
    public static List<string> ValidateTemplate(CoasterTemplate template)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(template.Name))
            errors.Add("Template name is required.");

        if (template.Settings.Diameter < 70 || template.Settings.Diameter > 150)
            errors.Add("Diameter must be between 70mm and 150mm.");

        if (template.Settings.BaseThickness < 2 || template.Settings.BaseThickness > 8)
            errors.Add("Base thickness must be between 2mm and 8mm.");

        if (template.Settings.TotalHeight < 3 || template.Settings.TotalHeight > 15)
            errors.Add("Total height must be between 3mm and 15mm.");

        return errors;
    }
}

/// <summary>
/// Built-in template presets.
/// </summary>
public static class BuiltInTemplates
{
    public static CoasterTemplate Minimalist => new()
    {
        Name = "Minimalist",
        Description = "Clean, simple circular coaster",
        Author = "CoaSTL",
        Tags = new List<string> { "simple", "modern", "circle" },
        Settings = new CoasterSettings
        {
            Shape = CoasterShape.Circle,
            Diameter = 90f,
            BaseThickness = 4f,
            TotalHeight = 5f,
            EdgeStyle = EdgeStyle.Flat
        },
        RecommendedMaterial = MaterialType.PLA
    };

    public static CoasterTemplate Hexagonal => new()
    {
        Name = "Hexagonal Modern",
        Description = "Modern hexagonal coaster with beveled edges",
        Author = "CoaSTL",
        Tags = new List<string> { "hexagon", "modern", "beveled" },
        Settings = new CoasterSettings
        {
            Shape = CoasterShape.Hexagon,
            Diameter = 100f,
            BaseThickness = 4f,
            TotalHeight = 6f,
            EdgeStyle = EdgeStyle.Beveled,
            BevelAngle = 45f
        },
        RecommendedMaterial = MaterialType.PETG
    };

    public static CoasterTemplate RoundedSquare => new()
    {
        Name = "Rounded Square",
        Description = "Square coaster with rounded corners",
        Author = "CoaSTL",
        Tags = new List<string> { "square", "rounded", "classic" },
        Settings = new CoasterSettings
        {
            Shape = CoasterShape.RoundedSquare,
            Diameter = 100f,
            BaseThickness = 4f,
            TotalHeight = 6f,
            EdgeStyle = EdgeStyle.Rounded,
            CornerRadius = 12f
        },
        RecommendedMaterial = MaterialType.PLA
    };

    public static CoasterTemplate Functional => new()
    {
        Name = "Functional Grip",
        Description = "Coaster with non-slip bottom and raised rim",
        Author = "CoaSTL",
        Tags = new List<string> { "functional", "grip", "rim" },
        Settings = new CoasterSettings
        {
            Shape = CoasterShape.Circle,
            Diameter = 100f,
            BaseThickness = 4f,
            TotalHeight = 8f,
            EdgeStyle = EdgeStyle.RaisedRim,
            AddNonSlipBottom = true
        },
        AdvancedSettings = new AdvancedCoasterSettings
        {
            AddDrainageGrooves = true,
            DrainageGrooveCount = 4,
            RimHeight = 3f,
            RimWidth = 4f
        },
        RecommendedMaterial = MaterialType.TPU
    };

    public static CoasterTemplate Octagonal => new()
    {
        Name = "Octagonal Classic",
        Description = "Classic octagonal coaster",
        Author = "CoaSTL",
        Tags = new List<string> { "octagon", "classic", "elegant" },
        Settings = new CoasterSettings
        {
            Shape = CoasterShape.Octagon,
            Diameter = 95f,
            BaseThickness = 4f,
            TotalHeight = 5f,
            EdgeStyle = EdgeStyle.Flat
        },
        RecommendedMaterial = MaterialType.WoodPLA
    };

    /// <summary>
    /// Gets all built-in templates.
    /// </summary>
    public static IEnumerable<CoasterTemplate> All => new[]
    {
        Minimalist, Hexagonal, RoundedSquare, Functional, Octagonal
    };

    /// <summary>
    /// Gets a template by name.
    /// </summary>
    public static CoasterTemplate? GetByName(string name)
    {
        return All.FirstOrDefault(t =>
            t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
