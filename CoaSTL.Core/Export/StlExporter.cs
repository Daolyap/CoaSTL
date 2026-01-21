using System.Text;
using CoaSTL.Core.Models;

namespace CoaSTL.Core.Export;

/// <summary>
/// STL file format options.
/// </summary>
public enum StlFormat
{
    Binary,
    Ascii
}

/// <summary>
/// Mesh resolution presets.
/// </summary>
public enum MeshResolution
{
    Draft,
    Standard,
    High,
    Ultra
}

/// <summary>
/// Options for STL export.
/// </summary>
public sealed class StlExportOptions
{
    /// <summary>
    /// Output format (binary or ASCII).
    /// </summary>
    public StlFormat Format { get; set; } = StlFormat.Binary;

    /// <summary>
    /// Name to include in the STL header.
    /// </summary>
    public string ModelName { get; set; } = "CoaSTL_Coaster";

    /// <summary>
    /// Include mesh statistics in ASCII comments.
    /// </summary>
    public bool IncludeStatistics { get; set; }
}

/// <summary>
/// Exports meshes to STL file format.
/// </summary>
public sealed class StlExporter
{
    /// <summary>
    /// Exports a mesh to STL format and saves to a file.
    /// </summary>
    public void Export(Mesh mesh, string filePath, StlExportOptions? options = null)
    {
        options ??= new StlExportOptions();

        using var stream = File.Create(filePath);
        Export(mesh, stream, options);
    }

    /// <summary>
    /// Exports a mesh to STL format and writes to a stream.
    /// </summary>
    public void Export(Mesh mesh, Stream stream, StlExportOptions? options = null)
    {
        options ??= new StlExportOptions();

        if (options.Format == StlFormat.Binary)
        {
            ExportBinary(mesh, stream, options);
        }
        else
        {
            ExportAscii(mesh, stream, options);
        }
    }

    /// <summary>
    /// Exports a mesh to binary STL format.
    /// </summary>
    private void ExportBinary(Mesh mesh, Stream stream, StlExportOptions options)
    {
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

        // Write 80-byte header
        var header = new byte[80];
        var headerText = Encoding.ASCII.GetBytes($"{options.ModelName} - Created by CoaSTL");
        Array.Copy(headerText, header, Math.Min(headerText.Length, 80));
        writer.Write(header);

        // Write triangle count (uint32)
        writer.Write((uint)mesh.TriangleCount);

        // Write each triangle
        foreach (var triangle in mesh.Triangles)
        {
            // Normal (3 floats)
            writer.Write(triangle.Normal.X);
            writer.Write(triangle.Normal.Y);
            writer.Write(triangle.Normal.Z);

            // Vertex 1 (3 floats)
            writer.Write(triangle.V1.X);
            writer.Write(triangle.V1.Y);
            writer.Write(triangle.V1.Z);

            // Vertex 2 (3 floats)
            writer.Write(triangle.V2.X);
            writer.Write(triangle.V2.Y);
            writer.Write(triangle.V2.Z);

            // Vertex 3 (3 floats)
            writer.Write(triangle.V3.X);
            writer.Write(triangle.V3.Y);
            writer.Write(triangle.V3.Z);

            // Attribute byte count (2 bytes, usually 0)
            writer.Write((ushort)0);
        }
    }

    /// <summary>
    /// Exports a mesh to ASCII STL format.
    /// </summary>
    private void ExportAscii(Mesh mesh, Stream stream, StlExportOptions options)
    {
        using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);

        // Write header
        writer.WriteLine($"solid {options.ModelName}");

        if (options.IncludeStatistics)
        {
            var (min, max) = mesh.GetBoundingBox();
            writer.WriteLine($"; Triangle count: {mesh.TriangleCount}");
            writer.WriteLine($"; Bounding box: ({min.X:F2}, {min.Y:F2}, {min.Z:F2}) to ({max.X:F2}, {max.Y:F2}, {max.Z:F2})");
            writer.WriteLine($"; Size: {max.X - min.X:F2} x {max.Y - min.Y:F2} x {max.Z - min.Z:F2} mm");
        }

        // Write each triangle
        foreach (var triangle in mesh.Triangles)
        {
            writer.WriteLine($"  facet normal {FormatFloat(triangle.Normal.X)} {FormatFloat(triangle.Normal.Y)} {FormatFloat(triangle.Normal.Z)}");
            writer.WriteLine("    outer loop");
            writer.WriteLine($"      vertex {FormatFloat(triangle.V1.X)} {FormatFloat(triangle.V1.Y)} {FormatFloat(triangle.V1.Z)}");
            writer.WriteLine($"      vertex {FormatFloat(triangle.V2.X)} {FormatFloat(triangle.V2.Y)} {FormatFloat(triangle.V2.Z)}");
            writer.WriteLine($"      vertex {FormatFloat(triangle.V3.X)} {FormatFloat(triangle.V3.Y)} {FormatFloat(triangle.V3.Z)}");
            writer.WriteLine("    endloop");
            writer.WriteLine("  endfacet");
        }

        writer.WriteLine($"endsolid {options.ModelName}");
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("E6", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Validates a mesh for STL export.
    /// </summary>
    public MeshValidationResult ValidateMesh(Mesh mesh)
    {
        var result = new MeshValidationResult();

        if (mesh.TriangleCount == 0)
        {
            result.AddError("Mesh has no triangles.");
            return result;
        }

        int degenerateCount = 0;
        int invertedNormalCount = 0;

        foreach (var triangle in mesh.Triangles)
        {
            // Check for degenerate triangles (zero area)
            var edge1 = triangle.V2 - triangle.V1;
            var edge2 = triangle.V3 - triangle.V1;
            var cross = Vector3.Cross(edge1, edge2);
            var area = cross.Length / 2;

            if (area < 1e-10f)
            {
                degenerateCount++;
            }

            // Check for NaN/Infinity
            if (float.IsNaN(triangle.Normal.X) || float.IsInfinity(triangle.Normal.X) ||
                float.IsNaN(triangle.Normal.Y) || float.IsInfinity(triangle.Normal.Y) ||
                float.IsNaN(triangle.Normal.Z) || float.IsInfinity(triangle.Normal.Z))
            {
                invertedNormalCount++;
            }
        }

        if (degenerateCount > 0)
        {
            result.AddWarning($"Mesh contains {degenerateCount} degenerate triangles (zero area).");
        }

        if (invertedNormalCount > 0)
        {
            result.AddWarning($"Mesh contains {invertedNormalCount} triangles with invalid normals.");
        }

        result.TriangleCount = mesh.TriangleCount;
        var (min, max) = mesh.GetBoundingBox();
        result.BoundingBoxMin = min;
        result.BoundingBoxMax = max;

        return result;
    }
}

/// <summary>
/// Result of mesh validation.
/// </summary>
public sealed class MeshValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public int TriangleCount { get; set; }
    public Vector3 BoundingBoxMin { get; set; }
    public Vector3 BoundingBoxMax { get; set; }

    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);

    public Vector3 Size => BoundingBoxMax - BoundingBoxMin;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Validation Result: {(IsValid ? "Valid" : "Invalid")}");
        sb.AppendLine($"Triangle Count: {TriangleCount}");
        sb.AppendLine($"Bounding Box: {BoundingBoxMin} to {BoundingBoxMax}");
        sb.AppendLine($"Size: {Size.X:F2} x {Size.Y:F2} x {Size.Z:F2} mm");

        if (Errors.Count > 0)
        {
            sb.AppendLine("Errors:");
            foreach (var error in Errors)
            {
                sb.AppendLine($"  - {error}");
            }
        }

        if (Warnings.Count > 0)
        {
            sb.AppendLine("Warnings:");
            foreach (var warning in Warnings)
            {
                sb.AppendLine($"  - {warning}");
            }
        }

        return sb.ToString();
    }
}
