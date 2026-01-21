using CoaSTL.Core;
using CoaSTL.Core.Export;
using CoaSTL.Core.Geometry;
using CoaSTL.Core.Models;
using CoaSTL.Core.Printers;

namespace CoaSTL.Tests;

public class Vector3Tests
{
    [Fact]
    public void Vector3_Constructor_SetsValues()
    {
        var v = new Vector3(1f, 2f, 3f);
        Assert.Equal(1f, v.X);
        Assert.Equal(2f, v.Y);
        Assert.Equal(3f, v.Z);
    }

    [Fact]
    public void Vector3_Zero_ReturnsZeroVector()
    {
        var v = Vector3.Zero;
        Assert.Equal(0f, v.X);
        Assert.Equal(0f, v.Y);
        Assert.Equal(0f, v.Z);
    }

    [Fact]
    public void Vector3_Length_CalculatesCorrectly()
    {
        var v = new Vector3(3f, 4f, 0f);
        Assert.Equal(5f, v.Length, 0.001f);
    }

    [Fact]
    public void Vector3_Normalized_ReturnsUnitVector()
    {
        var v = new Vector3(3f, 4f, 0f);
        var normalized = v.Normalized();
        Assert.Equal(1f, normalized.Length, 0.001f);
    }

    [Fact]
    public void Vector3_Cross_CalculatesCorrectly()
    {
        var a = Vector3.UnitX;
        var b = Vector3.UnitY;
        var cross = Vector3.Cross(a, b);
        Assert.True(cross == Vector3.UnitZ);
    }

    [Fact]
    public void Vector3_Addition_Works()
    {
        var a = new Vector3(1f, 2f, 3f);
        var b = new Vector3(4f, 5f, 6f);
        var result = a + b;
        Assert.True(result == new Vector3(5f, 7f, 9f));
    }

    [Fact]
    public void Vector3_Subtraction_Works()
    {
        var a = new Vector3(4f, 5f, 6f);
        var b = new Vector3(1f, 2f, 3f);
        var result = a - b;
        Assert.True(result == new Vector3(3f, 3f, 3f));
    }

    [Fact]
    public void Vector3_ScalarMultiplication_Works()
    {
        var v = new Vector3(1f, 2f, 3f);
        var result = v * 2f;
        Assert.True(result == new Vector3(2f, 4f, 6f));
    }
}

public class TriangleTests
{
    [Fact]
    public void Triangle_Constructor_CalculatesNormal()
    {
        var t = new Triangle(
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        );

        // Normal should point in +Z direction
        Assert.Equal(0f, t.Normal.X, 0.001f);
        Assert.Equal(0f, t.Normal.Y, 0.001f);
        Assert.Equal(1f, t.Normal.Z, 0.001f);
    }

    [Fact]
    public void Triangle_Flip_ReversesNormal()
    {
        var t = new Triangle(
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        );

        var flipped = t.Flip();

        Assert.Equal(0f, flipped.Normal.X, 0.001f);
        Assert.Equal(0f, flipped.Normal.Y, 0.001f);
        Assert.Equal(-1f, flipped.Normal.Z, 0.001f);
    }
}

public class MeshTests
{
    [Fact]
    public void Mesh_AddTriangle_IncreasesCount()
    {
        var mesh = new Mesh();
        Assert.Equal(0, mesh.TriangleCount);

        mesh.AddTriangle(
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        );

        Assert.Equal(1, mesh.TriangleCount);
    }

    [Fact]
    public void Mesh_AddQuad_AddsTwoTriangles()
    {
        var mesh = new Mesh();
        mesh.AddQuad(
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0)
        );

        Assert.Equal(2, mesh.TriangleCount);
    }

    [Fact]
    public void Mesh_GetBoundingBox_ReturnsCorrectBounds()
    {
        var mesh = new Mesh();
        mesh.AddTriangle(
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(0, 10, 5)
        );

        var (min, max) = mesh.GetBoundingBox();

        Assert.Equal(0f, min.X);
        Assert.Equal(0f, min.Y);
        Assert.Equal(0f, min.Z);
        Assert.Equal(10f, max.X);
        Assert.Equal(10f, max.Y);
        Assert.Equal(5f, max.Z);
    }

    [Fact]
    public void Mesh_Merge_CombinesMeshes()
    {
        var mesh1 = new Mesh();
        mesh1.AddTriangle(Vector3.Zero, Vector3.UnitX, Vector3.UnitY);

        var mesh2 = new Mesh();
        mesh2.AddTriangle(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);

        mesh1.Merge(mesh2);

        Assert.Equal(2, mesh1.TriangleCount);
    }
}

public class CoasterSettingsTests
{
    [Fact]
    public void CoasterSettings_DefaultValues_AreCorrect()
    {
        var settings = new CoasterSettings();

        Assert.Equal(CoasterShape.Circle, settings.Shape);
        Assert.Equal(100f, settings.Diameter);
        Assert.Equal(4f, settings.BaseThickness);
        Assert.Equal(6f, settings.TotalHeight);
        Assert.Equal(EdgeStyle.Flat, settings.EdgeStyle);
    }

    [Fact]
    public void CoasterSettings_Validate_ClampsDiameter()
    {
        var settings = new CoasterSettings { Diameter = 200f };
        settings.Validate();
        Assert.Equal(150f, settings.Diameter);

        settings.Diameter = 50f;
        settings.Validate();
        Assert.Equal(70f, settings.Diameter);
    }

    [Fact]
    public void CoasterSettings_Validate_ClampsThickness()
    {
        var settings = new CoasterSettings { BaseThickness = 10f };
        settings.Validate();
        Assert.Equal(8f, settings.BaseThickness);

        settings.BaseThickness = 1f;
        settings.Validate();
        Assert.Equal(2f, settings.BaseThickness);
    }
}

public class ShapeGeneratorTests
{
    [Fact]
    public void GenerateCircle_ReturnsCorrectPointCount()
    {
        var points = ShapeGenerator.GenerateCircle(50f, 64);
        Assert.Equal(64, points.Count);
    }

    [Fact]
    public void GenerateCircle_PointsAreAtCorrectRadius()
    {
        var radius = 50f;
        var points = ShapeGenerator.GenerateCircle(radius, 32);

        foreach (var (x, y) in points)
        {
            var distance = MathF.Sqrt(x * x + y * y);
            Assert.Equal(radius, distance, 0.001f);
        }
    }

    [Fact]
    public void GenerateSquare_ReturnsFourPoints()
    {
        var points = ShapeGenerator.GenerateSquare(100f);
        Assert.Equal(4, points.Count);
    }

    [Fact]
    public void GenerateHexagon_ReturnsSixPoints()
    {
        var points = ShapeGenerator.GenerateHexagon(50f);
        Assert.Equal(6, points.Count);
    }

    [Fact]
    public void GenerateOctagon_ReturnsEightPoints()
    {
        var points = ShapeGenerator.GenerateOctagon(50f);
        Assert.Equal(8, points.Count);
    }

    [Fact]
    public void GeneratePolygon_ReturnsCorrectPointCount()
    {
        var points = ShapeGenerator.GeneratePolygon(50f, 5);
        Assert.Equal(5, points.Count);
    }

    [Fact]
    public void GenerateProfile_UsesCorrectShape()
    {
        var settings = new CoasterSettings { Shape = CoasterShape.Square, Diameter = 100f };
        var points = ShapeGenerator.GenerateProfile(settings);
        Assert.Equal(4, points.Count);
    }
}

public class MeshGeneratorTests
{
    [Fact]
    public void GenerateCoaster_Circle_CreatesMesh()
    {
        var generator = new MeshGenerator();
        var settings = new CoasterSettings
        {
            Shape = CoasterShape.Circle,
            Diameter = 100f,
            BaseThickness = 4f,
            TotalHeight = 6f
        };

        var mesh = generator.GenerateCoaster(settings);

        Assert.True(mesh.TriangleCount > 0);
    }

    [Fact]
    public void GenerateCoaster_Square_CreatesMesh()
    {
        var generator = new MeshGenerator();
        var settings = new CoasterSettings
        {
            Shape = CoasterShape.Square,
            Diameter = 100f,
            BaseThickness = 4f,
            TotalHeight = 6f
        };

        var mesh = generator.GenerateCoaster(settings);

        Assert.True(mesh.TriangleCount > 0);
    }

    [Fact]
    public void GenerateCoaster_Hexagon_CreatesMesh()
    {
        var generator = new MeshGenerator();
        var settings = new CoasterSettings
        {
            Shape = CoasterShape.Hexagon,
            Diameter = 100f,
            BaseThickness = 4f,
            TotalHeight = 6f
        };

        var mesh = generator.GenerateCoaster(settings);

        Assert.True(mesh.TriangleCount > 0);
    }

    [Fact]
    public void GenerateCoaster_WithHeightMap_CreatesMesh()
    {
        var generator = new MeshGenerator();
        var settings = new CoasterSettings
        {
            Shape = CoasterShape.Circle,
            Diameter = 100f,
            BaseThickness = 4f,
            TotalHeight = 6f,
            ReliefDepth = 2f
        };

        // Create a simple height map
        var heightMap = new float[64, 64];
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                heightMap[i, j] = (float)(i + j) / 128f;
            }
        }

        var mesh = generator.GenerateCoaster(settings, heightMap);

        Assert.True(mesh.TriangleCount > 0);
    }

    [Fact]
    public void GenerateCoaster_BoundingBox_MatchesSettings()
    {
        var generator = new MeshGenerator();
        var settings = new CoasterSettings
        {
            Shape = CoasterShape.Circle,
            Diameter = 100f,
            BaseThickness = 4f,
            TotalHeight = 6f
        };

        var mesh = generator.GenerateCoaster(settings);
        var (min, max) = mesh.GetBoundingBox();

        // Check dimensions match approximately
        Assert.InRange(max.X - min.X, 95f, 105f); // Diameter
        Assert.InRange(max.Y - min.Y, 95f, 105f); // Diameter
        Assert.Equal(6f, max.Z - min.Z, 0.1f);    // Height
    }
}

public class StlExporterTests
{
    [Fact]
    public void Export_Binary_WritesCorrectHeader()
    {
        var exporter = new StlExporter();
        var mesh = CreateSimpleMesh();

        using var stream = new MemoryStream();
        exporter.Export(mesh, stream, new StlExportOptions { Format = StlFormat.Binary });

        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        // Read header (80 bytes)
        var header = reader.ReadBytes(80);
        Assert.Equal(80, header.Length);

        // Read triangle count
        var triangleCount = reader.ReadUInt32();
        Assert.Equal((uint)mesh.TriangleCount, triangleCount);
    }

    [Fact]
    public void Export_Binary_WritesCorrectTriangleCount()
    {
        var exporter = new StlExporter();
        var mesh = CreateSimpleMesh();

        using var stream = new MemoryStream();
        exporter.Export(mesh, stream, new StlExportOptions { Format = StlFormat.Binary });

        // Binary STL: 80 header + 4 triangle count + 50 bytes per triangle
        var expectedSize = 80 + 4 + mesh.TriangleCount * 50;
        Assert.Equal(expectedSize, stream.Length);
    }

    [Fact]
    public void Export_Ascii_ContainsSolidKeyword()
    {
        var exporter = new StlExporter();
        var mesh = CreateSimpleMesh();

        using var stream = new MemoryStream();
        exporter.Export(mesh, stream, new StlExportOptions { Format = StlFormat.Ascii, ModelName = "TestModel" });

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        Assert.Contains("solid TestModel", content);
        Assert.Contains("endsolid TestModel", content);
        Assert.Contains("facet normal", content);
        Assert.Contains("vertex", content);
    }

    [Fact]
    public void ValidateMesh_EmptyMesh_ReturnsInvalid()
    {
        var exporter = new StlExporter();
        var mesh = new Mesh();

        var result = exporter.ValidateMesh(mesh);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void ValidateMesh_ValidMesh_ReturnsValid()
    {
        var exporter = new StlExporter();
        var mesh = CreateSimpleMesh();

        var result = exporter.ValidateMesh(mesh);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    private static Mesh CreateSimpleMesh()
    {
        var mesh = new Mesh();
        mesh.AddTriangle(
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        );
        mesh.AddTriangle(
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1)
        );
        return mesh;
    }
}

public class ThreeMfExporterTests
{
    [Fact]
    public void Export_CreatesValidZipArchive()
    {
        var exporter = new ThreeMfExporter();
        var mesh = CreateSimpleMesh();

        using var stream = new MemoryStream();
        exporter.Export(mesh, stream, new ThreeMfExportOptions { ModelName = "TestModel" });

        // Verify it's a valid zip by trying to open it
        stream.Position = 0;
        using var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read);

        Assert.Contains(archive.Entries, e => e.FullName == "[Content_Types].xml");
        Assert.Contains(archive.Entries, e => e.FullName == "_rels/.rels");
        Assert.Contains(archive.Entries, e => e.FullName == "3D/3dmodel.model");
    }

    private static Mesh CreateSimpleMesh()
    {
        var mesh = new Mesh();
        mesh.AddTriangle(
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        );
        return mesh;
    }
}

public class BambuPrinterProfilesTests
{
    [Fact]
    public void All_ContainsAllPrinters()
    {
        var profiles = BambuPrinterProfiles.All.ToList();

        Assert.True(profiles.Count >= 6);
        Assert.Contains(profiles, p => p.ModelName == "X1 Carbon");
        Assert.Contains(profiles, p => p.ModelName == "X1E");
        Assert.Contains(profiles, p => p.ModelName == "P1P");
        Assert.Contains(profiles, p => p.ModelName == "P1S");
        Assert.Contains(profiles, p => p.ModelName == "A1");
        Assert.Contains(profiles, p => p.ModelName == "A1 mini");
    }

    [Fact]
    public void GetByName_FindsProfile()
    {
        var profile = BambuPrinterProfiles.GetByName("X1 Carbon");

        Assert.NotNull(profile);
        Assert.Equal("X1 Carbon", profile.ModelName);
    }

    [Fact]
    public void GetByName_CaseInsensitive()
    {
        var profile = BambuPrinterProfiles.GetByName("x1 carbon");

        Assert.NotNull(profile);
        Assert.Equal("X1 Carbon", profile.ModelName);
    }

    [Fact]
    public void GetByName_ReturnsNullForUnknown()
    {
        var profile = BambuPrinterProfiles.GetByName("Unknown Printer");

        Assert.Null(profile);
    }

    [Fact]
    public void ValidateCoasterFits_WithinBounds_ReturnsTrue()
    {
        var profile = BambuPrinterProfiles.X1Carbon;

        var fits = BambuPrinterProfiles.ValidateCoasterFits(profile, 100f, 10f);

        Assert.True(fits);
    }

    [Fact]
    public void ValidateCoasterFits_TooLarge_ReturnsFalse()
    {
        var profile = BambuPrinterProfiles.A1Mini; // 180mm build volume

        var fits = BambuPrinterProfiles.ValidateCoasterFits(profile, 200f, 10f);

        Assert.False(fits);
    }

    [Fact]
    public void X1Carbon_HasCorrectSpecs()
    {
        var profile = BambuPrinterProfiles.X1Carbon;

        Assert.Equal(256f, profile.BuildVolumeX);
        Assert.Equal(256f, profile.BuildVolumeY);
        Assert.Equal(256f, profile.BuildVolumeZ);
        Assert.True(profile.HasAms);
        Assert.Equal(16, profile.AmsSlots);
        Assert.True(profile.SupportsHighSpeed);
    }
}

public class MaterialPresetsTests
{
    [Fact]
    public void All_ContainsMaterials()
    {
        var materials = MaterialPresets.All.ToList();
        Assert.True(materials.Count >= 5);
    }

    [Fact]
    public void PLA_HasCorrectProperties()
    {
        var pla = MaterialPresets.PLA;
        Assert.Equal("PLA", pla.Name);
        Assert.Equal(MaterialType.PLA, pla.Type);
        Assert.Equal(1.24f, pla.Density);
    }

    [Fact]
    public void GetByName_FindsMaterial()
    {
        var material = MaterialPresets.GetByName("PETG");
        Assert.NotNull(material);
        Assert.Equal(MaterialType.PETG, material.Type);
    }

    [Fact]
    public void GetByType_FindsMaterial()
    {
        var material = MaterialPresets.GetByType(MaterialType.TPU);
        Assert.NotNull(material);
        Assert.Contains("TPU", material.Name);
    }
}

public class TemplateTests
{
    [Fact]
    public void BuiltInTemplates_ContainsTemplates()
    {
        var templates = BuiltInTemplates.All.ToList();
        Assert.True(templates.Count >= 4);
    }

    [Fact]
    public void GetByName_FindsTemplate()
    {
        var template = BuiltInTemplates.GetByName("Minimalist");
        Assert.NotNull(template);
        Assert.Equal("Minimalist", template.Name);
    }

    [Fact]
    public void SerializeTemplate_CreatesJson()
    {
        var template = new CoasterTemplate
        {
            Name = "Test",
            Settings = new CoasterSettings { Shape = CoasterShape.Hexagon }
        };

        var json = TemplateManager.SerializeTemplate(template);

        Assert.Contains("Test", json);
        Assert.Contains("hexagon", json.ToLowerInvariant());
    }

    [Fact]
    public void DeserializeTemplate_ReadsJson()
    {
        var template = new CoasterTemplate
        {
            Name = "Test",
            Settings = new CoasterSettings { Shape = CoasterShape.Hexagon }
        };

        var json = TemplateManager.SerializeTemplate(template);
        var loaded = TemplateManager.DeserializeTemplate(json);

        Assert.Equal("Test", loaded.Name);
        Assert.Equal(CoasterShape.Hexagon, loaded.Settings.Shape);
    }

    [Fact]
    public void ValidateTemplate_ReturnsErrors()
    {
        var template = new CoasterTemplate
        {
            Name = "",
            Settings = new CoasterSettings { Diameter = 200f }
        };

        var errors = TemplateManager.ValidateTemplate(template);

        Assert.True(errors.Count >= 2);
    }
}

public class CoasterDesignerTests
{
    [Fact]
    public void GenerateMesh_WithDefaultSettings_CreatesMesh()
    {
        using var designer = new CoasterDesigner();

        var mesh = designer.GenerateMesh();

        Assert.NotNull(mesh);
        Assert.True(mesh.TriangleCount > 0);
    }

    [Fact]
    public void GenerateMesh_WithCustomSettings_CreatesMesh()
    {
        using var designer = new CoasterDesigner();
        designer.Settings = new CoasterSettings
        {
            Shape = CoasterShape.Hexagon,
            Diameter = 90f,
            BaseThickness = 5f,
            TotalHeight = 8f
        };

        var mesh = designer.GenerateMesh();

        Assert.NotNull(mesh);
        Assert.True(mesh.TriangleCount > 0);
    }

    [Fact]
    public void ValidateMesh_AfterGeneration_ReturnsResult()
    {
        using var designer = new CoasterDesigner();
        designer.GenerateMesh();

        var result = designer.ValidateMesh();

        Assert.True(result.IsValid);
        Assert.True(result.TriangleCount > 0);
    }

    [Fact]
    public void ValidateMesh_BeforeGeneration_ThrowsException()
    {
        using var designer = new CoasterDesigner();

        Assert.Throws<InvalidOperationException>(() => designer.ValidateMesh());
    }

    [Fact]
    public void ExportToStl_AfterGeneration_WritesToStream()
    {
        using var designer = new CoasterDesigner();
        designer.GenerateMesh();

        using var stream = new MemoryStream();
        designer.ExportToStl(stream);

        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void ExportTo3Mf_AfterGeneration_WritesToStream()
    {
        using var designer = new CoasterDesigner();
        designer.GenerateMesh();

        using var stream = new MemoryStream();
        designer.ExportTo3Mf(stream);

        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void EstimateFilamentUsage_ReturnsPositiveValue()
    {
        using var designer = new CoasterDesigner();
        designer.GenerateMesh();

        var usage = designer.EstimateFilamentUsage();

        Assert.True(usage > 0);
    }

    [Fact]
    public void CalculateMaterialEstimate_ReturnsEstimate()
    {
        using var designer = new CoasterDesigner();
        designer.GenerateMesh();

        var estimate = designer.CalculateMaterialEstimate();

        Assert.True(estimate.WeightGrams > 0);
        Assert.True(estimate.EstimatedCost > 0);
    }

    [Fact]
    public void ValidateForPrinter_FitsOnPrinter_ReturnsTrue()
    {
        using var designer = new CoasterDesigner();
        designer.Settings = new CoasterSettings { Diameter = 100f };

        var fits = designer.ValidateForPrinter(BambuPrinterProfiles.X1Carbon);

        Assert.True(fits);
    }

    [Fact]
    public void AddText_AddsTextElement()
    {
        using var designer = new CoasterDesigner();
        designer.AddText(new TextElement { Text = "TEST" });

        Assert.Single(designer.AdvancedSettings.TextElements);
    }

    [Fact]
    public void ClearText_RemovesTextElements()
    {
        using var designer = new CoasterDesigner();
        designer.AddText(new TextElement { Text = "TEST" });
        designer.ClearText();

        Assert.Empty(designer.AdvancedSettings.TextElements);
    }

    [Fact]
    public void LoadFromTemplate_AppliesSettings()
    {
        using var designer = new CoasterDesigner();
        var template = BuiltInTemplates.Hexagonal;

        designer.LoadFromTemplate(template);

        Assert.Equal(CoasterShape.Hexagon, designer.Settings.Shape);
    }

    [Fact]
    public void SaveToTemplate_CreatesTemplate()
    {
        using var designer = new CoasterDesigner();
        designer.Settings = new CoasterSettings { Shape = CoasterShape.Octagon };

        var template = designer.SaveToTemplate("Test Template");

        Assert.Equal("Test Template", template.Name);
        Assert.Equal(CoasterShape.Octagon, template.Settings.Shape);
    }

    [Fact]
    public void Reset_ClearsState()
    {
        using var designer = new CoasterDesigner();
        designer.Settings = new CoasterSettings { Diameter = 120f };
        designer.AddText(new TextElement { Text = "TEST" });
        designer.GenerateMesh();

        designer.Reset();

        Assert.Equal(100f, designer.Settings.Diameter);
        Assert.Empty(designer.AdvancedSettings.TextElements);
        Assert.Null(designer.CurrentMesh);
    }
}

public class BatchProcessorTests
{
    [Fact]
    public void GenerateSet_CreatesMultipleFiles()
    {
        var processor = new BatchProcessor();
        var settings = new CoasterSettings
        {
            Shape = CoasterShape.Circle,
            Diameter = 80f
        };

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var config = new BatchGenerationConfig
            {
                OutputDirectory = tempDir,
                FileNamePattern = "test_{0:D2}.stl"
            };

            var result = processor.GenerateSet(settings, 2, config);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.SuccessCount);
            Assert.True(result.AllSucceeded);
            Assert.True(File.Exists(Path.Combine(tempDir, "test_01.stl")));
            Assert.True(File.Exists(Path.Combine(tempDir, "test_02.stl")));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}

public class MaterialCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsEstimate()
    {
        var calculator = new MaterialCalculator();
        var mesh = CreateCoasterMesh();
        var material = MaterialPresets.PLA;

        var estimate = calculator.Calculate(mesh, material);

        Assert.True(estimate.VolumeCm3 >= 0);
        Assert.True(estimate.WeightGrams >= 0);
    }

    private static Mesh CreateCoasterMesh()
    {
        var generator = new MeshGenerator();
        var settings = new CoasterSettings
        {
            Shape = CoasterShape.Circle,
            Diameter = 100f
        };
        return generator.GenerateCoaster(settings);
    }
}

public class TextGeneratorTests
{
    [Fact]
    public void GenerateText_CreatesTriangles()
    {
        var textElement = new TextElement
        {
            Text = "A",
            FontSize = 10f,
            Depth = 1f,
            Embossed = true
        };

        var triangles = TextGenerator.GenerateText(textElement, 100f, 6f);

        Assert.True(triangles.Count > 0);
    }

    [Fact]
    public void GenerateText_EmptyString_ReturnsEmpty()
    {
        var textElement = new TextElement { Text = "" };

        var triangles = TextGenerator.GenerateText(textElement, 100f, 6f);

        Assert.Empty(triangles);
    }
}