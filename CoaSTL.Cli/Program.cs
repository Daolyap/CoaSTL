using CoaSTL.Core;
using CoaSTL.Core.Export;
using CoaSTL.Core.Models;
using CoaSTL.Core.Printers;

namespace CoaSTL.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        Console.WriteLine($"CoaSTL v{AssemblyInfo.Version} - 3D Printable Coaster Designer");
        Console.WriteLine("=====================================================");
        Console.WriteLine();

        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }

        try
        {
            return ProcessCommand(args);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    private static int ProcessCommand(string[] args)
    {
        var command = args[0].ToLowerInvariant();

        return command switch
        {
            "generate" => GenerateCoaster(args.Skip(1).ToArray()),
            "batch" => GenerateBatch(args.Skip(1).ToArray()),
            "printers" => ListPrinters(),
            "shapes" => ListShapes(),
            "materials" => ListMaterials(),
            "templates" => ListTemplates(),
            "validate" => ValidateStl(args.Skip(1).ToArray()),
            "version" or "-v" or "--version" => ShowVersion(),
            "help" or "--help" or "-h" => ShowHelp(),
            _ => InvalidCommand(command)
        };
    }

    private static int GenerateCoaster(string[] args)
    {
        var settings = new CoasterSettings();
        var advancedSettings = new AdvancedCoasterSettings();
        var outputPath = "coaster.stl";
        string? imagePath = null;
        var format = StlFormat.Binary;
        string? printerName = null;
        string? materialName = null;
        string? text = null;
        bool export3mf = false;
        string? templateName = null;

        // Parse arguments
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLowerInvariant();
            var nextArg = i + 1 < args.Length ? args[i + 1] : null;

            switch (arg)
            {
                case "-o" or "--output":
                    if (nextArg != null) outputPath = nextArg;
                    i++;
                    break;
                case "-s" or "--shape":
                    if (nextArg != null && Enum.TryParse<CoasterShape>(nextArg, true, out var shape))
                        settings.Shape = shape;
                    i++;
                    break;
                case "-d" or "--diameter":
                    if (nextArg != null && float.TryParse(nextArg, out var diameter))
                        settings.Diameter = diameter;
                    i++;
                    break;
                case "-t" or "--thickness":
                    if (nextArg != null && float.TryParse(nextArg, out var thickness))
                        settings.BaseThickness = thickness;
                    i++;
                    break;
                case "--total-height":
                    if (nextArg != null && float.TryParse(nextArg, out var height))
                        settings.TotalHeight = height;
                    i++;
                    break;
                case "-e" or "--edge":
                    if (nextArg != null && Enum.TryParse<EdgeStyle>(nextArg, true, out var edge))
                        settings.EdgeStyle = edge;
                    i++;
                    break;
                case "-i" or "--image":
                    imagePath = nextArg;
                    i++;
                    break;
                case "-r" or "--relief":
                    if (nextArg != null && float.TryParse(nextArg, out var relief))
                        settings.ReliefDepth = relief;
                    i++;
                    break;
                case "--invert":
                    settings.InvertRelief = true;
                    break;
                case "--nonslip":
                    settings.AddNonSlipBottom = true;
                    break;
                case "--ascii":
                    format = StlFormat.Ascii;
                    break;
                case "--3mf":
                    export3mf = true;
                    break;
                case "--corners":
                    if (nextArg != null && float.TryParse(nextArg, out var corners))
                        settings.CornerRadius = corners;
                    i++;
                    break;
                case "--sides":
                    if (nextArg != null && int.TryParse(nextArg, out var sides))
                        settings.PolygonSides = sides;
                    i++;
                    break;
                case "-p" or "--printer":
                    printerName = nextArg;
                    i++;
                    break;
                case "-m" or "--material":
                    materialName = nextArg;
                    i++;
                    break;
                case "--text":
                    text = nextArg;
                    i++;
                    break;
                case "--drainage":
                    advancedSettings.AddDrainageGrooves = true;
                    break;
                case "--template":
                    templateName = nextArg;
                    i++;
                    break;
            }
        }

        using var designer = new CoasterDesigner();

        // Load from template if specified
        if (!string.IsNullOrEmpty(templateName))
        {
            var template = BuiltInTemplates.GetByName(templateName);
            if (template != null)
            {
                designer.LoadFromTemplate(template);
                Console.WriteLine($"Loaded template: {template.Name}");
            }
            else if (File.Exists(templateName))
            {
                var fileTemplate = TemplateManager.LoadTemplate(templateName);
                designer.LoadFromTemplate(fileTemplate);
                Console.WriteLine($"Loaded template from: {templateName}");
            }
        }
        else
        {
            designer.Settings = settings;
            designer.AdvancedSettings = advancedSettings;
        }

        Console.WriteLine($"Generating {designer.Settings.Shape} coaster...");
        Console.WriteLine($"  Diameter: {designer.Settings.Diameter}mm");
        Console.WriteLine($"  Thickness: {designer.Settings.BaseThickness}mm");
        Console.WriteLine($"  Total Height: {designer.Settings.TotalHeight}mm");
        Console.WriteLine($"  Edge Style: {designer.Settings.EdgeStyle}");

        // Add text if specified
        if (!string.IsNullOrEmpty(text))
        {
            designer.AddText(new TextElement
            {
                Text = text,
                FontSize = 8f,
                Depth = 1f,
                Embossed = true,
                Alignment = TextAlignment.Center
            });
            Console.WriteLine($"  Text: {text}");
        }

        // Load and process image if provided
        if (!string.IsNullOrEmpty(imagePath))
        {
            Console.WriteLine($"  Loading image: {imagePath}");
            designer.LoadImage(imagePath);
            designer.ProcessImage(grayscale: true);
            designer.GenerateHeightMap(128);
            Console.WriteLine($"  Relief Depth: {designer.Settings.ReliefDepth}mm");
        }

        // Validate for printer if specified
        BambuPrinterProfile? printerProfile = null;
        if (!string.IsNullOrEmpty(printerName))
        {
            printerProfile = BambuPrinterProfiles.GetByName(printerName);
            if (printerProfile != null)
            {
                if (!designer.ValidateForPrinter(printerProfile))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Coaster may not fit on {printerProfile.ModelName}!");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  Target Printer: {printerProfile.ModelName}");
                }
            }
        }

        // Generate mesh
        designer.GenerateMesh();
        var result = designer.ValidateMesh();

        Console.WriteLine();
        Console.WriteLine($"Mesh generated: {result.TriangleCount} triangles");
        Console.WriteLine($"Bounding box: {result.BoundingBoxMin} to {result.BoundingBoxMax}");
        Console.WriteLine($"Size: {result.Size.X:F2} x {result.Size.Y:F2} x {result.Size.Z:F2} mm");

        if (!result.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warnings:");
            foreach (var warning in result.Warnings)
                Console.WriteLine($"  - {warning}");
            foreach (var error in result.Errors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  - ERROR: {error}");
            }
            Console.ResetColor();
        }

        // Export
        if (export3mf)
        {
            var mfPath = Path.ChangeExtension(outputPath, ".3mf");
            designer.ExportTo3Mf(mfPath, new ThreeMfExportOptions
            {
                ModelName = Path.GetFileNameWithoutExtension(mfPath),
                IncludeColorInfo = true
            });
            Console.WriteLine($"\nExported to: {mfPath}");
            Console.WriteLine("Format: 3MF");
        }
        else
        {
            var options = new StlExportOptions
            {
                Format = format,
                ModelName = Path.GetFileNameWithoutExtension(outputPath),
                IncludeStatistics = format == StlFormat.Ascii
            };
            designer.ExportToStl(outputPath, options);
            Console.WriteLine($"\nExported to: {outputPath}");
            Console.WriteLine($"Format: {format}");
        }

        // Material estimates
        var material = materialName != null
            ? MaterialPresets.GetByName(materialName) ?? MaterialPresets.PLA
            : MaterialPresets.PLA;

        var estimate = designer.CalculateMaterialEstimate(material);
        Console.WriteLine($"\nMaterial Estimates ({material.Name}):");
        Console.WriteLine($"  Weight: {estimate.WeightGrams:F1}g");
        Console.WriteLine($"  Filament: {estimate.FilamentLengthMeters:F2}m");
        Console.WriteLine($"  Cost: ${estimate.EstimatedCost:F2}");
        Console.WriteLine($"  Print Time: ~{estimate.PrintTimeMinutes} min");

        if (printerProfile != null)
        {
            var printTime = designer.EstimatePrintTime(printerProfile);
            Console.WriteLine($"  Estimated print time on {printerProfile.ModelName}: ~{printTime} min");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Coaster generated successfully!");
        Console.ResetColor();

        return 0;
    }

    private static int GenerateBatch(string[] args)
    {
        var settings = new CoasterSettings();
        var outputDir = ".";
        int count = 4;
        string? pattern = null;

        // Parse arguments
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLowerInvariant();
            var nextArg = i + 1 < args.Length ? args[i + 1] : null;

            switch (arg)
            {
                case "-o" or "--output":
                    if (nextArg != null) outputDir = nextArg;
                    i++;
                    break;
                case "-c" or "--count":
                    if (nextArg != null && int.TryParse(nextArg, out var c))
                        count = c;
                    i++;
                    break;
                case "-s" or "--shape":
                    if (nextArg != null && Enum.TryParse<CoasterShape>(nextArg, true, out var shape))
                        settings.Shape = shape;
                    i++;
                    break;
                case "-d" or "--diameter":
                    if (nextArg != null && float.TryParse(nextArg, out var diameter))
                        settings.Diameter = diameter;
                    i++;
                    break;
                case "--pattern":
                    pattern = nextArg;
                    i++;
                    break;
            }
        }

        Console.WriteLine($"Batch generating {count} coasters...");
        Console.WriteLine($"Output directory: {outputDir}");
        Console.WriteLine();

        var processor = new BatchProcessor();
        var config = new BatchGenerationConfig
        {
            OutputDirectory = outputDir,
            FileNamePattern = pattern ?? "coaster_{0:D3}.stl",
            ProgressCallback = progress =>
            {
                Console.Write($"\rProgress: {progress}%");
            }
        };

        var result = processor.GenerateSet(settings, count, config);

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"Batch complete: {result.SuccessCount}/{result.TotalCount} successful");
        Console.WriteLine($"Time: {result.ElapsedMilliseconds}ms");

        if (result.FailedCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Failed: {result.FailedCount}");
            foreach (var item in result.Items.Where(i => !i.Success))
            {
                Console.WriteLine($"  - Item {item.Index + 1}: {item.Error}");
            }
            Console.ResetColor();
        }

        return result.AllSucceeded ? 0 : 1;
    }

    private static int ListPrinters()
    {
        Console.WriteLine("Available Bambu Labs Printer Profiles:");
        Console.WriteLine();

        foreach (var profile in BambuPrinterProfiles.All)
        {
            Console.WriteLine($"  {profile.ModelName}");
            Console.WriteLine($"    Build Volume: {profile.BuildVolumeX} x {profile.BuildVolumeY} x {profile.BuildVolumeZ} mm");
            Console.WriteLine($"    AMS Support: {(profile.HasAms ? $"Yes ({profile.AmsSlots} slots)" : "No")}");
            Console.WriteLine($"    Recommended Speed: {profile.RecommendedSpeed} mm/s");
            Console.WriteLine();
        }

        return 0;
    }

    private static int ListShapes()
    {
        Console.WriteLine("Available Coaster Shapes:");
        Console.WriteLine();

        foreach (var shape in Enum.GetValues<CoasterShape>())
        {
            Console.WriteLine($"  {shape}");
        }

        Console.WriteLine();
        Console.WriteLine("Available Edge Styles:");
        Console.WriteLine();

        foreach (var edge in Enum.GetValues<EdgeStyle>())
        {
            Console.WriteLine($"  {edge}");
        }

        return 0;
    }

    private static int ListMaterials()
    {
        Console.WriteLine("Available Material Presets:");
        Console.WriteLine();

        foreach (var material in MaterialPresets.All)
        {
            Console.WriteLine($"  {material.Name}");
            Console.WriteLine($"    Density: {material.Density} g/cm³");
            Console.WriteLine($"    Price: ${material.PricePerKg}/kg");
            Console.WriteLine($"    Nozzle Temp: {material.NozzleTemp}°C");
            Console.WriteLine($"    Bed Temp: {material.BedTemp}°C");
            Console.WriteLine($"    Food Safe: {(material.FoodSafe ? "Yes" : "No")}");
            Console.WriteLine($"    Heat Resistant: {(material.HeatResistant ? "Yes" : "No")}");
            Console.WriteLine($"    {material.Description}");
            Console.WriteLine();
        }

        return 0;
    }

    private static int ListTemplates()
    {
        Console.WriteLine("Built-in Templates:");
        Console.WriteLine();

        foreach (var template in BuiltInTemplates.All)
        {
            Console.WriteLine($"  {template.Name}");
            Console.WriteLine($"    {template.Description}");
            Console.WriteLine($"    Shape: {template.Settings.Shape}, Diameter: {template.Settings.Diameter}mm");
            Console.WriteLine($"    Tags: {string.Join(", ", template.Tags)}");
            Console.WriteLine($"    Recommended Material: {template.RecommendedMaterial}");
            Console.WriteLine();
        }

        return 0;
    }

    private static int ValidateStl(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: CoaSTL.Cli validate <stl-file>");
            return 1;
        }

        var filePath = args[0];

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return 1;
        }

        Console.WriteLine($"Validating: {filePath}");
        Console.WriteLine();

        // Read STL and parse
        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"File Size: {fileInfo.Length / 1024.0:F1} KB");

        // Basic validation - check if it's a valid STL
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // Check header
        var header = reader.ReadBytes(80);
        var triangleCount = reader.ReadUInt32();

        Console.WriteLine($"Triangle Count: {triangleCount}");

        // Calculate expected file size
        var expectedSize = 80 + 4 + triangleCount * 50;
        var isBinary = fileInfo.Length == expectedSize;

        Console.WriteLine($"Format: {(isBinary ? "Binary" : "ASCII")}");

        if (isBinary)
        {
            // Validate binary STL
            Console.WriteLine("\nValidation:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Valid binary STL structure");
            Console.ResetColor();
        }

        return 0;
    }

    private static int ShowVersion()
    {
        Console.WriteLine($"CoaSTL Version {AssemblyInfo.Version}");
        Console.WriteLine(AssemblyInfo.Description);
        Console.WriteLine(AssemblyInfo.Copyright);
        return 0;
    }

    private static int ShowHelp()
    {
        Console.WriteLine("Usage: CoaSTL.Cli <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  generate    Generate a coaster STL/3MF file");
        Console.WriteLine("  batch       Generate multiple coasters at once");
        Console.WriteLine("  printers    List available Bambu printer profiles");
        Console.WriteLine("  shapes      List available shapes and edge styles");
        Console.WriteLine("  materials   List available material presets");
        Console.WriteLine("  templates   List built-in design templates");
        Console.WriteLine("  validate    Validate an STL file");
        Console.WriteLine("  version     Show version information");
        Console.WriteLine("  help        Show this help message");
        Console.WriteLine();
        Console.WriteLine("Generate Options:");
        Console.WriteLine("  -o, --output <file>      Output file path (default: coaster.stl)");
        Console.WriteLine("  -s, --shape <shape>      Coaster shape (Circle, Square, Hexagon, Octagon, RoundedSquare, CustomPolygon)");
        Console.WriteLine("  -d, --diameter <mm>      Diameter/width (70-150mm, default: 100)");
        Console.WriteLine("  -t, --thickness <mm>     Base thickness (2-8mm, default: 4)");
        Console.WriteLine("  --total-height <mm>      Total height (3-15mm, default: 6)");
        Console.WriteLine("  -e, --edge <style>       Edge style (Flat, Beveled, Rounded, RaisedRim)");
        Console.WriteLine("  -i, --image <file>       Image file for relief (PNG, JPG, BMP, etc.)");
        Console.WriteLine("  -r, --relief <mm>        Relief depth (0.5-5mm, default: 1.5)");
        Console.WriteLine("  --invert                 Invert relief (debossed instead of embossed)");
        Console.WriteLine("  --nonslip                Add non-slip pattern on bottom");
        Console.WriteLine("  --ascii                  Export as ASCII STL (default: binary)");
        Console.WriteLine("  --3mf                    Export as 3MF format");
        Console.WriteLine("  --corners <mm>           Corner radius for RoundedSquare");
        Console.WriteLine("  --sides <n>              Number of sides for CustomPolygon (3-12)");
        Console.WriteLine("  -p, --printer <name>     Validate for specific Bambu printer");
        Console.WriteLine("  -m, --material <name>    Material for cost estimation (PLA, PETG, etc.)");
        Console.WriteLine("  --text <text>            Add embossed text");
        Console.WriteLine("  --drainage               Add drainage grooves");
        Console.WriteLine("  --template <name>        Use a built-in or custom template");
        Console.WriteLine();
        Console.WriteLine("Batch Options:");
        Console.WriteLine("  -o, --output <dir>       Output directory");
        Console.WriteLine("  -c, --count <n>          Number of coasters to generate");
        Console.WriteLine("  -s, --shape <shape>      Coaster shape");
        Console.WriteLine("  -d, --diameter <mm>      Diameter/width");
        Console.WriteLine("  --pattern <pattern>      File name pattern (use {0} for index)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  CoaSTL.Cli generate -o mycoaster.stl -s Circle -d 100");
        Console.WriteLine("  CoaSTL.Cli generate -s Hexagon -i logo.png -r 2.0");
        Console.WriteLine("  CoaSTL.Cli generate --template \"Minimalist\"");
        Console.WriteLine("  CoaSTL.Cli generate -s Circle --text \"HELLO\" --3mf");
        Console.WriteLine("  CoaSTL.Cli batch -c 6 -s Hexagon -o ./coasters/");
        Console.WriteLine("  CoaSTL.Cli validate coaster.stl");
        Console.WriteLine();

        return 0;
    }

    private static int InvalidCommand(string command)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unknown command: {command}");
        Console.ResetColor();
        Console.WriteLine("Use 'CoaSTL.Cli help' to see available commands.");
        return 1;
    }
}
