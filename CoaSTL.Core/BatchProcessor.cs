using CoaSTL.Core.Export;
using CoaSTL.Core.Models;

namespace CoaSTL.Core;

/// <summary>
/// Result of a batch generation operation.
/// </summary>
public sealed class BatchGenerationResult
{
    /// <summary>
    /// Total number of coasters to generate.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of successfully generated coasters.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed generations.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Individual results for each coaster.
    /// </summary>
    public List<BatchItemResult> Items { get; set; } = new();

    /// <summary>
    /// Total generation time in milliseconds.
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Whether all coasters were generated successfully.
    /// </summary>
    public bool AllSucceeded => FailedCount == 0;
}

/// <summary>
/// Result of a single batch item generation.
/// </summary>
public sealed class BatchItemResult
{
    /// <summary>
    /// Index of this item in the batch.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Name of the generated file.
    /// </summary>
    public string FileName { get; set; } = "";

    /// <summary>
    /// Whether the generation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if generation failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Mesh validation result.
    /// </summary>
    public MeshValidationResult? ValidationResult { get; set; }
}

/// <summary>
/// Configuration for batch generation.
/// </summary>
public sealed class BatchGenerationConfig
{
    /// <summary>
    /// Output directory for generated files.
    /// </summary>
    public string OutputDirectory { get; set; } = ".";

    /// <summary>
    /// File name pattern (use {0} for index, {1} for name).
    /// </summary>
    public string FileNamePattern { get; set; } = "coaster_{0:D3}.stl";

    /// <summary>
    /// STL export options.
    /// </summary>
    public StlExportOptions StlOptions { get; set; } = new();

    /// <summary>
    /// Whether to stop on first error.
    /// </summary>
    public bool StopOnError { get; set; }

    /// <summary>
    /// Progress callback (reports 0-100).
    /// </summary>
    public Action<int>? ProgressCallback { get; set; }
}

/// <summary>
/// Handles batch generation of multiple coasters.
/// </summary>
public sealed class BatchProcessor
{
    /// <summary>
    /// Generates a set of identical coasters.
    /// </summary>
    public BatchGenerationResult GenerateSet(
        CoasterSettings settings,
        int count,
        BatchGenerationConfig config)
    {
        var items = Enumerable.Range(0, count)
            .Select(_ => settings)
            .ToList();

        return GenerateBatch(items, config);
    }

    /// <summary>
    /// Generates coasters from a list of settings.
    /// </summary>
    public BatchGenerationResult GenerateBatch(
        IList<CoasterSettings> settingsList,
        BatchGenerationConfig config)
    {
        var result = new BatchGenerationResult
        {
            TotalCount = settingsList.Count
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Ensure output directory exists
        if (!Directory.Exists(config.OutputDirectory))
        {
            Directory.CreateDirectory(config.OutputDirectory);
        }

        for (int i = 0; i < settingsList.Count; i++)
        {
            var itemResult = new BatchItemResult { Index = i };

            try
            {
                using var designer = new CoasterDesigner();
                designer.Settings = settingsList[i];

                var fileName = string.Format(config.FileNamePattern, i + 1, settingsList[i].Shape);
                var filePath = Path.Combine(config.OutputDirectory, fileName);

                var validation = designer.GenerateAndExport(filePath, config.StlOptions);

                itemResult.FileName = fileName;
                itemResult.Success = validation.IsValid;
                itemResult.ValidationResult = validation;

                if (validation.IsValid)
                {
                    result.SuccessCount++;
                }
                else
                {
                    result.FailedCount++;
                    itemResult.Error = string.Join("; ", validation.Errors);
                }
            }
            catch (Exception ex)
            {
                itemResult.Success = false;
                itemResult.Error = ex.Message;
                result.FailedCount++;

                if (config.StopOnError)
                {
                    result.Items.Add(itemResult);
                    break;
                }
            }

            result.Items.Add(itemResult);

            // Report progress
            config.ProgressCallback?.Invoke((i + 1) * 100 / settingsList.Count);
        }

        stopwatch.Stop();
        result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        return result;
    }

    /// <summary>
    /// Generates personalized coasters from a list of text values.
    /// </summary>
    public BatchGenerationResult GeneratePersonalized(
        CoasterSettings baseSettings,
        IList<string> textValues,
        AdvancedCoasterSettings advancedSettings,
        BatchGenerationConfig config)
    {
        var settingsList = new List<CoasterSettings>();

        foreach (var text in textValues)
        {
            var settings = new CoasterSettings
            {
                Shape = baseSettings.Shape,
                Diameter = baseSettings.Diameter,
                BaseThickness = baseSettings.BaseThickness,
                TotalHeight = baseSettings.TotalHeight,
                EdgeStyle = baseSettings.EdgeStyle,
                BevelAngle = baseSettings.BevelAngle,
                CornerRadius = baseSettings.CornerRadius,
                PolygonSides = baseSettings.PolygonSides,
                ReliefDepth = baseSettings.ReliefDepth,
                InvertRelief = baseSettings.InvertRelief,
                AddNonSlipBottom = baseSettings.AddNonSlipBottom,
                CurveResolution = baseSettings.CurveResolution
            };

            // The text is stored in advancedSettings.TextElements
            // For batch, we'll need to update this per item
            settingsList.Add(settings);
        }

        // Update config to include index/name in file pattern
        if (config.FileNamePattern == "coaster_{0:D3}.stl")
        {
            config.FileNamePattern = "coaster_{0:D3}_{1}.stl";
        }

        return GenerateBatch(settingsList, config);
    }

    /// <summary>
    /// Generates coasters from templates.
    /// </summary>
    public BatchGenerationResult GenerateFromTemplates(
        IList<CoasterTemplate> templates,
        BatchGenerationConfig config)
    {
        var settingsList = templates.Select(t => t.Settings).ToList();
        return GenerateBatch(settingsList, config);
    }
}

/// <summary>
/// Material cost calculator.
/// </summary>
public sealed class MaterialCalculator
{
    // Shape efficiency constant (π/4 for circular shapes)
    private const float CircleShapeEfficiency = 0.785f;

    // Percentage of volume that is solid walls/surfaces
    private const float SolidWallsPercentage = 0.3f;

    // Percentage of volume that is infill
    private const float InfillPercentage = 0.7f;

    /// <summary>
    /// Calculates material estimates for a coaster.
    /// </summary>
    public MaterialEstimate Calculate(Mesh mesh, Material material, float infillDensity = 0.2f)
    {
        var (min, max) = mesh.GetBoundingBox();
        var size = max - min;

        // Calculate approximate volume (bounding box * fill factor)
        // Real volume would require mesh volume calculation
        var boundingVolume = size.X * size.Y * size.Z;

        // Estimate actual volume (coaster shape + infill)
        // Circular coaster: ~78.5% of bounding box, then * infill
        var solidVolume = boundingVolume * CircleShapeEfficiency * SolidWallsPercentage;
        var infillVolume = boundingVolume * CircleShapeEfficiency * InfillPercentage * infillDensity;
        var totalVolumeMm3 = solidVolume + infillVolume;
        var volumeCm3 = totalVolumeMm3 / 1000f;

        // Weight calculation
        var weightGrams = volumeCm3 * material.Density;

        // Filament length (1.75mm diameter)
        var filamentRadius = 0.875f; // mm
        var filamentCrossSection = MathF.PI * filamentRadius * filamentRadius; // mm²
        var filamentLengthMm = totalVolumeMm3 / filamentCrossSection;
        var filamentLengthMeters = filamentLengthMm / 1000f;

        // Cost
        var costPerGram = material.PricePerKg / 1000m;
        var estimatedCost = (decimal)weightGrams * costPerGram;

        // Print time estimation (very rough)
        // Based on typical print speeds and layer heights
        var layerHeight = 0.2f; // mm
        var layers = size.Z / layerHeight;
        var avgLayerTime = 30f; // seconds per layer (rough estimate)
        var printTimeMinutes = (int)(layers * avgLayerTime / 60f);

        return new MaterialEstimate
        {
            Material = material,
            VolumeCm3 = volumeCm3,
            WeightGrams = weightGrams,
            FilamentLengthMeters = filamentLengthMeters,
            EstimatedCost = Math.Round(estimatedCost, 2),
            PrintTimeMinutes = printTimeMinutes
        };
    }
}
