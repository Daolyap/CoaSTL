namespace CoaSTL.Core.Models;

/// <summary>
/// Material types for 3D printing.
/// </summary>
public enum MaterialType
{
    PLA,
    PETG,
    TPU,
    ABS,
    ASA,
    WoodPLA,
    MarblePLA,
    SilkPLA,
    CarbonFiberPLA,
    Custom
}

/// <summary>
/// Represents a 3D printing material with its properties.
/// </summary>
public sealed class Material
{
    /// <summary>
    /// Material name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Material type.
    /// </summary>
    public MaterialType Type { get; init; }

    /// <summary>
    /// Density in g/cm³.
    /// </summary>
    public float Density { get; init; }

    /// <summary>
    /// Price per kilogram in USD.
    /// </summary>
    public decimal PricePerKg { get; init; }

    /// <summary>
    /// Recommended nozzle temperature in °C.
    /// </summary>
    public int NozzleTemp { get; init; }

    /// <summary>
    /// Recommended bed temperature in °C.
    /// </summary>
    public int BedTemp { get; init; }

    /// <summary>
    /// Recommended print speed in mm/s.
    /// </summary>
    public int PrintSpeed { get; init; }

    /// <summary>
    /// Whether the material is food-safe.
    /// </summary>
    public bool FoodSafe { get; init; }

    /// <summary>
    /// Whether the material is heat-resistant.
    /// </summary>
    public bool HeatResistant { get; init; }

    /// <summary>
    /// Color in hex format (e.g., "#FFFFFF").
    /// </summary>
    public string Color { get; init; } = "#FFFFFF";

    /// <summary>
    /// Description of the material.
    /// </summary>
    public string Description { get; init; } = "";
}

/// <summary>
/// Pre-defined material presets.
/// </summary>
public static class MaterialPresets
{
    public static Material PLA => new()
    {
        Name = "PLA",
        Type = MaterialType.PLA,
        Density = 1.24f,
        PricePerKg = 20m,
        NozzleTemp = 210,
        BedTemp = 60,
        PrintSpeed = 60,
        FoodSafe = false,
        HeatResistant = false,
        Description = "Standard PLA - Easy to print, biodegradable"
    };

    public static Material PETG => new()
    {
        Name = "PETG",
        Type = MaterialType.PETG,
        Density = 1.27f,
        PricePerKg = 25m,
        NozzleTemp = 240,
        BedTemp = 80,
        PrintSpeed = 50,
        FoodSafe = true,
        HeatResistant = true,
        Description = "PETG - Durable, food-safe, heat-resistant"
    };

    public static Material TPU => new()
    {
        Name = "TPU (Flexible)",
        Type = MaterialType.TPU,
        Density = 1.21f,
        PricePerKg = 35m,
        NozzleTemp = 220,
        BedTemp = 50,
        PrintSpeed = 30,
        FoodSafe = false,
        HeatResistant = false,
        Description = "TPU - Flexible, rubber-like, shock-absorbing"
    };

    public static Material ABS => new()
    {
        Name = "ABS",
        Type = MaterialType.ABS,
        Density = 1.04f,
        PricePerKg = 22m,
        NozzleTemp = 250,
        BedTemp = 100,
        PrintSpeed = 50,
        FoodSafe = false,
        HeatResistant = true,
        Description = "ABS - Strong, heat-resistant, requires enclosure"
    };

    public static Material ASA => new()
    {
        Name = "ASA",
        Type = MaterialType.ASA,
        Density = 1.07f,
        PricePerKg = 28m,
        NozzleTemp = 250,
        BedTemp = 100,
        PrintSpeed = 50,
        FoodSafe = false,
        HeatResistant = true,
        Description = "ASA - UV-resistant, outdoor use, similar to ABS"
    };

    public static Material WoodPLA => new()
    {
        Name = "Wood PLA",
        Type = MaterialType.WoodPLA,
        Density = 1.15f,
        PricePerKg = 30m,
        NozzleTemp = 200,
        BedTemp = 60,
        PrintSpeed = 40,
        FoodSafe = false,
        HeatResistant = false,
        Description = "Wood PLA - Contains wood fibers, natural appearance"
    };

    public static Material SilkPLA => new()
    {
        Name = "Silk PLA",
        Type = MaterialType.SilkPLA,
        Density = 1.24f,
        PricePerKg = 25m,
        NozzleTemp = 215,
        BedTemp = 60,
        PrintSpeed = 50,
        FoodSafe = false,
        HeatResistant = false,
        Description = "Silk PLA - Shiny metallic finish"
    };

    /// <summary>
    /// Gets all available material presets.
    /// </summary>
    public static IEnumerable<Material> All => new[]
    {
        PLA, PETG, TPU, ABS, ASA, WoodPLA, SilkPLA
    };

    /// <summary>
    /// Gets a material by type.
    /// </summary>
    public static Material? GetByType(MaterialType type)
    {
        return All.FirstOrDefault(m => m.Type == type);
    }

    /// <summary>
    /// Gets a material by name (case-insensitive).
    /// </summary>
    public static Material? GetByName(string name)
    {
        return All.FirstOrDefault(m =>
            m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            m.Type.ToString().Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Result of material cost calculation.
/// </summary>
public sealed class MaterialEstimate
{
    /// <summary>
    /// Material used.
    /// </summary>
    public Material Material { get; init; } = MaterialPresets.PLA;

    /// <summary>
    /// Estimated volume in cubic centimeters.
    /// </summary>
    public float VolumeCm3 { get; init; }

    /// <summary>
    /// Estimated weight in grams.
    /// </summary>
    public float WeightGrams { get; init; }

    /// <summary>
    /// Estimated filament length in meters.
    /// </summary>
    public float FilamentLengthMeters { get; init; }

    /// <summary>
    /// Estimated cost in USD.
    /// </summary>
    public decimal EstimatedCost { get; init; }

    /// <summary>
    /// Estimated print time in minutes.
    /// </summary>
    public int PrintTimeMinutes { get; init; }

    public override string ToString()
    {
        return $"Material: {Material.Name}\n" +
               $"Volume: {VolumeCm3:F2} cm³\n" +
               $"Weight: {WeightGrams:F1}g\n" +
               $"Filament: {FilamentLengthMeters:F2}m\n" +
               $"Cost: ${EstimatedCost:F2}\n" +
               $"Print Time: ~{PrintTimeMinutes} min";
    }
}
