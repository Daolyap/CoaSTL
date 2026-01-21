using CoaSTL.Core;
using CoaSTL.Core.Export;
using CoaSTL.Core.Models;
using CoaSTL.Core.Printers;

namespace CoaSTL.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        Console.WriteLine("CoaSTL - 3D Printable Coaster Designer");
        Console.WriteLine("========================================");
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
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static int ProcessCommand(string[] args)
    {
        var command = args[0].ToLowerInvariant();

        return command switch
        {
            "generate" => GenerateCoaster(args.Skip(1).ToArray()),
            "printers" => ListPrinters(),
            "shapes" => ListShapes(),
            "help" or "--help" or "-h" => ShowHelp(),
            _ => InvalidCommand(command)
        };
    }

    private static int GenerateCoaster(string[] args)
    {
        var settings = new CoasterSettings();
        var outputPath = "coaster.stl";
        string? imagePath = null;
        var format = StlFormat.Binary;
        string? printerName = null;

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
                case "-h" or "--height":
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
            }
        }

        using var designer = new CoasterDesigner();
        designer.Settings = settings;

        Console.WriteLine($"Generating {settings.Shape} coaster...");
        Console.WriteLine($"  Diameter: {settings.Diameter}mm");
        Console.WriteLine($"  Thickness: {settings.BaseThickness}mm");
        Console.WriteLine($"  Total Height: {settings.TotalHeight}mm");
        Console.WriteLine($"  Edge Style: {settings.EdgeStyle}");

        // Load and process image if provided
        if (!string.IsNullOrEmpty(imagePath))
        {
            Console.WriteLine($"  Loading image: {imagePath}");
            designer.LoadImage(imagePath);
            designer.ProcessImage(grayscale: true);
            designer.GenerateHeightMap(128); // Default resolution
            Console.WriteLine($"  Relief Depth: {settings.ReliefDepth}mm");
        }

        // Validate for printer if specified
        if (!string.IsNullOrEmpty(printerName))
        {
            var profile = BambuPrinterProfiles.GetByName(printerName);
            if (profile != null)
            {
                if (!designer.ValidateForPrinter(profile))
                {
                    Console.WriteLine($"Warning: Coaster may not fit on {profile.ModelName}!");
                }
                else
                {
                    Console.WriteLine($"  Target Printer: {profile.ModelName}");
                }
            }
        }

        // Generate and export
        var options = new StlExportOptions
        {
            Format = format,
            ModelName = Path.GetFileNameWithoutExtension(outputPath),
            IncludeStatistics = format == StlFormat.Ascii
        };

        var result = designer.GenerateAndExport(outputPath, options);

        Console.WriteLine();
        Console.WriteLine($"Mesh generated: {result.TriangleCount} triangles");
        Console.WriteLine($"Bounding box: {result.BoundingBoxMin} to {result.BoundingBoxMax}");
        Console.WriteLine($"Size: {result.Size.X:F2} x {result.Size.Y:F2} x {result.Size.Z:F2} mm");

        if (!result.IsValid)
        {
            Console.WriteLine("Warnings:");
            foreach (var warning in result.Warnings)
                Console.WriteLine($"  - {warning}");
            foreach (var error in result.Errors)
                Console.WriteLine($"  - ERROR: {error}");
        }

        Console.WriteLine();
        Console.WriteLine($"Exported to: {outputPath}");
        Console.WriteLine($"Format: {format}");

        // Show estimates
        var filamentGrams = designer.EstimateFilamentUsage();
        Console.WriteLine($"Estimated filament: {filamentGrams:F1}g");

        return 0;
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

    private static int ShowHelp()
    {
        Console.WriteLine("Usage: CoaSTL.Cli <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  generate    Generate a coaster STL file");
        Console.WriteLine("  printers    List available Bambu printer profiles");
        Console.WriteLine("  shapes      List available shapes and edge styles");
        Console.WriteLine("  help        Show this help message");
        Console.WriteLine();
        Console.WriteLine("Generate Options:");
        Console.WriteLine("  -o, --output <file>      Output STL file path (default: coaster.stl)");
        Console.WriteLine("  -s, --shape <shape>      Coaster shape (Circle, Square, Hexagon, Octagon, RoundedSquare, CustomPolygon)");
        Console.WriteLine("  -d, --diameter <mm>      Diameter/width (70-150mm, default: 100)");
        Console.WriteLine("  -t, --thickness <mm>     Base thickness (2-8mm, default: 4)");
        Console.WriteLine("  -h, --height <mm>        Total height (3-15mm, default: 6)");
        Console.WriteLine("  -e, --edge <style>       Edge style (Flat, Beveled, Rounded, RaisedRim)");
        Console.WriteLine("  -i, --image <file>       Image file for relief (PNG, JPG, BMP, etc.)");
        Console.WriteLine("  -r, --relief <mm>        Relief depth (0.5-5mm, default: 1.5)");
        Console.WriteLine("  --invert                 Invert relief (debossed instead of embossed)");
        Console.WriteLine("  --nonslip                Add non-slip pattern on bottom");
        Console.WriteLine("  --ascii                  Export as ASCII STL (default: binary)");
        Console.WriteLine("  --corners <mm>           Corner radius for RoundedSquare");
        Console.WriteLine("  --sides <n>              Number of sides for CustomPolygon (3-12)");
        Console.WriteLine("  -p, --printer <name>     Validate for specific Bambu printer");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  CoaSTL.Cli generate -o mycoaster.stl -s Circle -d 100");
        Console.WriteLine("  CoaSTL.Cli generate -s Hexagon -i logo.png -r 2.0");
        Console.WriteLine("  CoaSTL.Cli generate -s RoundedSquare --corners 15 -p \"A1 mini\"");
        Console.WriteLine();

        return 0;
    }

    private static int InvalidCommand(string command)
    {
        Console.WriteLine($"Unknown command: {command}");
        Console.WriteLine("Use 'CoaSTL.Cli help' to see available commands.");
        return 1;
    }
}
