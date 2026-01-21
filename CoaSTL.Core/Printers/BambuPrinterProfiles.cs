namespace CoaSTL.Core.Printers;

/// <summary>
/// Represents a Bambu Labs printer profile.
/// </summary>
public sealed class BambuPrinterProfile
{
    /// <summary>
    /// Printer model name.
    /// </summary>
    public string ModelName { get; init; } = "";

    /// <summary>
    /// Maximum build volume X (mm).
    /// </summary>
    public float BuildVolumeX { get; init; }

    /// <summary>
    /// Maximum build volume Y (mm).
    /// </summary>
    public float BuildVolumeY { get; init; }

    /// <summary>
    /// Maximum build volume Z (mm).
    /// </summary>
    public float BuildVolumeZ { get; init; }

    /// <summary>
    /// Whether the printer has AMS (Automatic Material System) support.
    /// </summary>
    public bool HasAms { get; init; }

    /// <summary>
    /// Maximum number of AMS slots.
    /// </summary>
    public int AmsSlots { get; init; }

    /// <summary>
    /// Recommended layer height for detailed prints (mm).
    /// </summary>
    public float DetailedLayerHeight { get; init; } = 0.12f;

    /// <summary>
    /// Standard layer height (mm).
    /// </summary>
    public float StandardLayerHeight { get; init; } = 0.2f;

    /// <summary>
    /// Recommended print speed for coasters (mm/s).
    /// </summary>
    public float RecommendedSpeed { get; init; }

    /// <summary>
    /// Whether the printer supports high-speed printing.
    /// </summary>
    public bool SupportsHighSpeed { get; init; }
}

/// <summary>
/// Provides pre-configured profiles for Bambu Labs printers.
/// </summary>
public static class BambuPrinterProfiles
{
    /// <summary>
    /// Bambu Lab X1 Carbon profile.
    /// </summary>
    public static BambuPrinterProfile X1Carbon => new()
    {
        ModelName = "X1 Carbon",
        BuildVolumeX = 256,
        BuildVolumeY = 256,
        BuildVolumeZ = 256,
        HasAms = true,
        AmsSlots = 16,
        DetailedLayerHeight = 0.12f,
        StandardLayerHeight = 0.2f,
        RecommendedSpeed = 250,
        SupportsHighSpeed = true
    };

    /// <summary>
    /// Bambu Lab X1E profile.
    /// </summary>
    public static BambuPrinterProfile X1E => new()
    {
        ModelName = "X1E",
        BuildVolumeX = 256,
        BuildVolumeY = 256,
        BuildVolumeZ = 256,
        HasAms = true,
        AmsSlots = 16,
        DetailedLayerHeight = 0.12f,
        StandardLayerHeight = 0.2f,
        RecommendedSpeed = 250,
        SupportsHighSpeed = true
    };

    /// <summary>
    /// Bambu Lab P1P profile.
    /// </summary>
    public static BambuPrinterProfile P1P => new()
    {
        ModelName = "P1P",
        BuildVolumeX = 256,
        BuildVolumeY = 256,
        BuildVolumeZ = 256,
        HasAms = true,
        AmsSlots = 4,
        DetailedLayerHeight = 0.12f,
        StandardLayerHeight = 0.2f,
        RecommendedSpeed = 250,
        SupportsHighSpeed = true
    };

    /// <summary>
    /// Bambu Lab P1S profile.
    /// </summary>
    public static BambuPrinterProfile P1S => new()
    {
        ModelName = "P1S",
        BuildVolumeX = 256,
        BuildVolumeY = 256,
        BuildVolumeZ = 256,
        HasAms = true,
        AmsSlots = 4,
        DetailedLayerHeight = 0.12f,
        StandardLayerHeight = 0.2f,
        RecommendedSpeed = 250,
        SupportsHighSpeed = true
    };

    /// <summary>
    /// Bambu Lab P2S profile (newest model with improved features).
    /// </summary>
    public static BambuPrinterProfile P2S => new()
    {
        ModelName = "P2S",
        BuildVolumeX = 256,
        BuildVolumeY = 256,
        BuildVolumeZ = 256,
        HasAms = true,
        AmsSlots = 4,
        DetailedLayerHeight = 0.08f,
        StandardLayerHeight = 0.16f,
        RecommendedSpeed = 300,
        SupportsHighSpeed = true
    };

    /// <summary>
    /// Bambu Lab A1 profile.
    /// </summary>
    public static BambuPrinterProfile A1 => new()
    {
        ModelName = "A1",
        BuildVolumeX = 256,
        BuildVolumeY = 256,
        BuildVolumeZ = 256,
        HasAms = true,
        AmsSlots = 4,
        DetailedLayerHeight = 0.12f,
        StandardLayerHeight = 0.2f,
        RecommendedSpeed = 250,
        SupportsHighSpeed = true
    };

    /// <summary>
    /// Bambu Lab A1 Mini profile.
    /// </summary>
    public static BambuPrinterProfile A1Mini => new()
    {
        ModelName = "A1 mini",
        BuildVolumeX = 180,
        BuildVolumeY = 180,
        BuildVolumeZ = 180,
        HasAms = true,
        AmsSlots = 4,
        DetailedLayerHeight = 0.12f,
        StandardLayerHeight = 0.2f,
        RecommendedSpeed = 200,
        SupportsHighSpeed = true
    };

    /// <summary>
    /// Gets all available printer profiles.
    /// </summary>
    public static IEnumerable<BambuPrinterProfile> All => new[]
    {
        X1Carbon, X1E, P1P, P1S, P2S, A1, A1Mini
    };

    /// <summary>
    /// Gets a printer profile by model name.
    /// </summary>
    public static BambuPrinterProfile? GetByName(string modelName)
    {
        return All.FirstOrDefault(p =>
            p.ModelName.Equals(modelName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates if a coaster fits within the printer's build volume.
    /// </summary>
    public static bool ValidateCoasterFits(BambuPrinterProfile profile, float diameter, float height)
    {
        return diameter <= profile.BuildVolumeX &&
               diameter <= profile.BuildVolumeY &&
               height <= profile.BuildVolumeZ;
    }
}
