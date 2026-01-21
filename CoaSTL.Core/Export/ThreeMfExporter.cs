using System.IO.Compression;
using System.Text;
using System.Xml;
using CoaSTL.Core.Models;

namespace CoaSTL.Core.Export;

/// <summary>
/// Options for 3MF export.
/// </summary>
public sealed class ThreeMfExportOptions
{
    /// <summary>
    /// Model name.
    /// </summary>
    public string ModelName { get; set; } = "CoaSTL_Coaster";

    /// <summary>
    /// Model description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Application name for metadata.
    /// </summary>
    public string Application { get; set; } = "CoaSTL";

    /// <summary>
    /// Include color/material information.
    /// </summary>
    public bool IncludeColorInfo { get; set; }

    /// <summary>
    /// Primary color (hex format).
    /// </summary>
    public string PrimaryColor { get; set; } = "#808080";

    /// <summary>
    /// Include thumbnail in the 3MF file.
    /// </summary>
    public bool IncludeThumbnail { get; set; }
}

/// <summary>
/// Exports meshes to 3MF file format (3D Manufacturing Format).
/// </summary>
public sealed class ThreeMfExporter
{
    private const string CoreNamespace = "http://schemas.microsoft.com/3dmanufacturing/core/2015/02";
    private const string MaterialNamespace = "http://schemas.microsoft.com/3dmanufacturing/material/2015/02";

    /// <summary>
    /// Exports a mesh to 3MF format and saves to a file.
    /// </summary>
    public void Export(Mesh mesh, string filePath, ThreeMfExportOptions? options = null)
    {
        options ??= new ThreeMfExportOptions();

        using var stream = File.Create(filePath);
        Export(mesh, stream, options);
    }

    /// <summary>
    /// Exports a mesh to 3MF format and writes to a stream.
    /// </summary>
    public void Export(Mesh mesh, Stream stream, ThreeMfExportOptions? options = null)
    {
        options ??= new ThreeMfExportOptions();

        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);

        // Add content types
        AddContentTypes(archive);

        // Add relationships
        AddRelationships(archive);

        // Add 3D model
        Add3DModel(archive, mesh, options);
    }

    private void AddContentTypes(ZipArchive archive)
    {
        var entry = archive.CreateEntry("[Content_Types].xml");
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);

        writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        writer.WriteLine("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
        writer.WriteLine("  <Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>");
        writer.WriteLine("  <Default Extension=\"model\" ContentType=\"application/vnd.ms-package.3dmanufacturing-3dmodel+xml\"/>");
        writer.WriteLine("</Types>");
    }

    private void AddRelationships(ZipArchive archive)
    {
        var entry = archive.CreateEntry("_rels/.rels");
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);

        writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        writer.WriteLine("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
        writer.WriteLine("  <Relationship Target=\"/3D/3dmodel.model\" Id=\"rel0\" Type=\"http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel\"/>");
        writer.WriteLine("</Relationships>");
    }

    private void Add3DModel(ZipArchive archive, Mesh mesh, ThreeMfExportOptions options)
    {
        var entry = archive.CreateEntry("3D/3dmodel.model");
        using var stream = entry.Open();

        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8
        };

        using var writer = XmlWriter.Create(stream, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("model", CoreNamespace);
        writer.WriteAttributeString("unit", "millimeter");
        writer.WriteAttributeString("xml", "lang", null, "en-US");

        if (options.IncludeColorInfo)
        {
            writer.WriteAttributeString("xmlns", "m", null, MaterialNamespace);
        }

        // Metadata
        WriteMetadata(writer, options);

        // Resources
        writer.WriteStartElement("resources");

        // Base material (if color info included)
        if (options.IncludeColorInfo)
        {
            WriteMaterials(writer, options);
        }

        // Object (mesh)
        WriteObject(writer, mesh, options);

        writer.WriteEndElement(); // resources

        // Build
        writer.WriteStartElement("build");
        writer.WriteStartElement("item");
        writer.WriteAttributeString("objectid", "1");
        writer.WriteEndElement(); // item
        writer.WriteEndElement(); // build

        writer.WriteEndElement(); // model
        writer.WriteEndDocument();
    }

    private void WriteMetadata(XmlWriter writer, ThreeMfExportOptions options)
    {
        writer.WriteStartElement("metadata");
        writer.WriteAttributeString("name", "Title");
        writer.WriteString(options.ModelName);
        writer.WriteEndElement();

        writer.WriteStartElement("metadata");
        writer.WriteAttributeString("name", "Designer");
        writer.WriteString(options.Application);
        writer.WriteEndElement();

        writer.WriteStartElement("metadata");
        writer.WriteAttributeString("name", "Description");
        writer.WriteString(options.Description);
        writer.WriteEndElement();

        writer.WriteStartElement("metadata");
        writer.WriteAttributeString("name", "CreationDate");
        writer.WriteString(DateTime.UtcNow.ToString("yyyy-MM-dd"));
        writer.WriteEndElement();

        writer.WriteStartElement("metadata");
        writer.WriteAttributeString("name", "Application");
        writer.WriteString($"{options.Application} v{AssemblyInfo.Version}");
        writer.WriteEndElement();
    }

    private void WriteMaterials(XmlWriter writer, ThreeMfExportOptions options)
    {
        writer.WriteStartElement("basematerials", MaterialNamespace);
        writer.WriteAttributeString("id", "1");

        writer.WriteStartElement("base", MaterialNamespace);
        writer.WriteAttributeString("name", "Material");
        writer.WriteAttributeString("displaycolor", options.PrimaryColor);
        writer.WriteEndElement();

        writer.WriteEndElement(); // basematerials
    }

    private void WriteObject(XmlWriter writer, Mesh mesh, ThreeMfExportOptions options)
    {
        writer.WriteStartElement("object");
        writer.WriteAttributeString("id", "1");
        writer.WriteAttributeString("name", options.ModelName);
        writer.WriteAttributeString("type", "model");

        if (options.IncludeColorInfo)
        {
            writer.WriteAttributeString("pid", "1");
            writer.WriteAttributeString("pindex", "0");
        }

        writer.WriteStartElement("mesh");

        // Vertices
        WriteVertices(writer, mesh);

        // Triangles
        WriteTriangles(writer, mesh);

        writer.WriteEndElement(); // mesh
        writer.WriteEndElement(); // object
    }

    private void WriteVertices(XmlWriter writer, Mesh mesh)
    {
        // Build unique vertex list
        var vertices = new List<Vector3>();
        var vertexIndices = new Dictionary<Vector3, int>(new Vector3Comparer());

        foreach (var triangle in mesh.Triangles)
        {
            foreach (var v in new[] { triangle.V1, triangle.V2, triangle.V3 })
            {
                if (!vertexIndices.ContainsKey(v))
                {
                    vertexIndices[v] = vertices.Count;
                    vertices.Add(v);
                }
            }
        }

        writer.WriteStartElement("vertices");

        foreach (var v in vertices)
        {
            writer.WriteStartElement("vertex");
            writer.WriteAttributeString("x", v.X.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", v.Y.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteAttributeString("z", v.Z.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        writer.WriteEndElement(); // vertices
    }

    private void WriteTriangles(XmlWriter writer, Mesh mesh)
    {
        // Build vertex index map
        var vertexIndices = new Dictionary<Vector3, int>(new Vector3Comparer());
        int index = 0;

        foreach (var triangle in mesh.Triangles)
        {
            foreach (var v in new[] { triangle.V1, triangle.V2, triangle.V3 })
            {
                if (!vertexIndices.ContainsKey(v))
                {
                    vertexIndices[v] = index++;
                }
            }
        }

        writer.WriteStartElement("triangles");

        foreach (var triangle in mesh.Triangles)
        {
            writer.WriteStartElement("triangle");
            writer.WriteAttributeString("v1", vertexIndices[triangle.V1].ToString());
            writer.WriteAttributeString("v2", vertexIndices[triangle.V2].ToString());
            writer.WriteAttributeString("v3", vertexIndices[triangle.V3].ToString());
            writer.WriteEndElement();
        }

        writer.WriteEndElement(); // triangles
    }

    /// <summary>
    /// Comparer for Vector3 to use in dictionaries.
    /// </summary>
    private class Vector3Comparer : IEqualityComparer<Vector3>
    {
        private const float Tolerance = 1e-6f;

        public bool Equals(Vector3 x, Vector3 y)
        {
            return MathF.Abs(x.X - y.X) < Tolerance &&
                   MathF.Abs(x.Y - y.Y) < Tolerance &&
                   MathF.Abs(x.Z - y.Z) < Tolerance;
        }

        public int GetHashCode(Vector3 obj)
        {
            return HashCode.Combine(
                MathF.Round(obj.X * 1000),
                MathF.Round(obj.Y * 1000),
                MathF.Round(obj.Z * 1000));
        }
    }
}

/// <summary>
/// Assembly information helper.
/// </summary>
public static class AssemblyInfo
{
    public static string Version => "1.0.0";
    public static string Name => "CoaSTL";
    public static string Description => "3D Printable Coaster Designer";
    public static string Copyright => $"Copyright © {DateTime.Now.Year} CoaSTL";
}
