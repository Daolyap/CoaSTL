using CoaSTL.Core.Export;
using CoaSTL.Core.Geometry;
using CoaSTL.Core.ImageProcessing;
using CoaSTL.Core.Models;
using CoaSTL.Core.Printers;

namespace CoaSTL.Core;

/// <summary>
/// Main class for designing and generating coaster STL files.
/// </summary>
public sealed class CoasterDesigner : IDisposable
{
    private readonly MeshGenerator _meshGenerator = new();
    private readonly ImageProcessor _imageProcessor = new();
    private readonly StlExporter _stlExporter = new();
    private CoasterSettings _settings = new();
    private float[,]? _heightMap;
    private Mesh? _currentMesh;
    private bool _disposed;

    /// <summary>
    /// Gets or sets the coaster settings.
    /// </summary>
    public CoasterSettings Settings
    {
        get => _settings;
        set => _settings = value ?? new CoasterSettings();
    }

    /// <summary>
    /// Gets the current mesh if generated.
    /// </summary>
    public Mesh? CurrentMesh => _currentMesh;

    /// <summary>
    /// Gets whether an image is loaded for relief generation.
    /// </summary>
    public bool HasImage => _imageProcessor.HasImage;

    /// <summary>
    /// Gets whether a mesh has been generated.
    /// </summary>
    public bool HasMesh => _currentMesh != null;

    /// <summary>
    /// Loads an image for relief generation.
    /// </summary>
    public void LoadImage(string filePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _imageProcessor.LoadImage(filePath);
    }

    /// <summary>
    /// Loads an image from a stream.
    /// </summary>
    public void LoadImage(Stream stream)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _imageProcessor.LoadImage(stream);
    }

    /// <summary>
    /// Processes the loaded image for height map generation.
    /// </summary>
    public void ProcessImage(bool grayscale = true, float? brightness = null,
        float? contrast = null, bool invert = false, float? blur = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (grayscale)
        {
            _imageProcessor.ConvertToGrayscale();
        }

        if (brightness.HasValue)
        {
            _imageProcessor.AdjustBrightness(brightness.Value);
        }

        if (contrast.HasValue)
        {
            _imageProcessor.AdjustContrast(contrast.Value);
        }

        if (invert)
        {
            _imageProcessor.Invert();
        }

        if (blur.HasValue && blur.Value > 0)
        {
            _imageProcessor.ApplyBlur(blur.Value);
        }
    }

    /// <summary>
    /// Generates a height map from the loaded image.
    /// </summary>
    public void GenerateHeightMap(int? resolution = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (resolution.HasValue)
        {
            _heightMap = _imageProcessor.GenerateHeightMap(resolution.Value, resolution.Value);
        }
        else
        {
            _heightMap = _imageProcessor.GenerateHeightMap();
        }
    }

    /// <summary>
    /// Generates the coaster mesh with current settings.
    /// </summary>
    public Mesh GenerateMesh()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _currentMesh = _meshGenerator.GenerateCoaster(_settings, _heightMap);
        return _currentMesh;
    }

    /// <summary>
    /// Validates the current mesh for 3D printing.
    /// </summary>
    public MeshValidationResult ValidateMesh()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_currentMesh == null)
            throw new InvalidOperationException("No mesh generated. Call GenerateMesh first.");

        return _stlExporter.ValidateMesh(_currentMesh);
    }

    /// <summary>
    /// Validates the coaster fits within a printer's build volume.
    /// </summary>
    public bool ValidateForPrinter(BambuPrinterProfile profile)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return BambuPrinterProfiles.ValidateCoasterFits(profile, _settings.Diameter, _settings.TotalHeight);
    }

    /// <summary>
    /// Exports the current mesh to an STL file.
    /// </summary>
    public void ExportToStl(string filePath, StlExportOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_currentMesh == null)
            throw new InvalidOperationException("No mesh generated. Call GenerateMesh first.");

        _stlExporter.Export(_currentMesh, filePath, options);
    }

    /// <summary>
    /// Exports the current mesh to a stream.
    /// </summary>
    public void ExportToStl(Stream stream, StlExportOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_currentMesh == null)
            throw new InvalidOperationException("No mesh generated. Call GenerateMesh first.");

        _stlExporter.Export(_currentMesh, stream, options);
    }

    /// <summary>
    /// Generates and exports a coaster in one operation.
    /// </summary>
    public MeshValidationResult GenerateAndExport(string outputPath, StlExportOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        GenerateMesh();
        var validation = ValidateMesh();

        if (validation.IsValid)
        {
            ExportToStl(outputPath, options);
        }

        return validation;
    }

    /// <summary>
    /// Calculates estimated filament usage in grams (assuming PLA density).
    /// </summary>
    public float EstimateFilamentUsage()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_currentMesh == null)
            throw new InvalidOperationException("No mesh generated. Call GenerateMesh first.");

        // Approximate volume calculation from bounding box
        // More accurate would be to calculate actual mesh volume
        var (min, max) = _currentMesh.GetBoundingBox();
        var size = max - min;

        // Estimate volume with rough 70% fill factor for coaster shape
        var volumeCubicMm = size.X * size.Y * size.Z * 0.7f;
        var volumeCubicCm = volumeCubicMm / 1000f;

        // PLA density approximately 1.24 g/cm³
        return volumeCubicCm * 1.24f;
    }

    /// <summary>
    /// Estimates print time in minutes based on printer profile.
    /// </summary>
    public int EstimatePrintTime(BambuPrinterProfile profile)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_currentMesh == null)
            throw new InvalidOperationException("No mesh generated. Call GenerateMesh first.");

        // Rough estimation based on coaster dimensions and print speed
        var layers = _settings.TotalHeight / profile.StandardLayerHeight;

        // Estimate perimeter and infill time per layer
        var perimeter = MathF.PI * _settings.Diameter; // For circular, approximation for others
        var perimeterTime = perimeter / profile.RecommendedSpeed / 60f;

        // Total estimate with buffer
        var totalMinutes = layers * perimeterTime * 10; // 10x multiplier for actual print complexity
        return (int)MathF.Ceiling(totalMinutes);
    }

    /// <summary>
    /// Resets the designer to initial state.
    /// </summary>
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _settings = new CoasterSettings();
        _heightMap = null;
        _currentMesh = null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _imageProcessor.Dispose();
            _disposed = true;
        }
    }
}
