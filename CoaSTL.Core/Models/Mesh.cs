namespace CoaSTL.Core.Models;

/// <summary>
/// Represents a 3D mesh composed of triangles.
/// </summary>
public sealed class Mesh
{
    private readonly List<Triangle> _triangles = new();

    public IReadOnlyList<Triangle> Triangles => _triangles;

    public int TriangleCount => _triangles.Count;

    public Mesh()
    {
    }

    public Mesh(IEnumerable<Triangle> triangles)
    {
        _triangles.AddRange(triangles);
    }

    public void AddTriangle(Triangle triangle)
    {
        _triangles.Add(triangle);
    }

    public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        _triangles.Add(new Triangle(v1, v2, v3));
    }

    public void AddTriangles(IEnumerable<Triangle> triangles)
    {
        _triangles.AddRange(triangles);
    }

    public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        // Split quad into two triangles (counter-clockwise winding)
        AddTriangle(v1, v2, v3);
        AddTriangle(v1, v3, v4);
    }

    /// <summary>
    /// Merges another mesh into this mesh.
    /// </summary>
    public void Merge(Mesh other)
    {
        _triangles.AddRange(other.Triangles);
    }

    /// <summary>
    /// Gets the bounding box of the mesh.
    /// </summary>
    public (Vector3 Min, Vector3 Max) GetBoundingBox()
    {
        if (_triangles.Count == 0)
            return (Vector3.Zero, Vector3.Zero);

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var minZ = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var maxZ = float.MinValue;

        foreach (var tri in _triangles)
        {
            foreach (var v in new[] { tri.V1, tri.V2, tri.V3 })
            {
                minX = MathF.Min(minX, v.X);
                minY = MathF.Min(minY, v.Y);
                minZ = MathF.Min(minZ, v.Z);
                maxX = MathF.Max(maxX, v.X);
                maxY = MathF.Max(maxY, v.Y);
                maxZ = MathF.Max(maxZ, v.Z);
            }
        }

        return (new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }

    /// <summary>
    /// Translates all vertices by the specified offset.
    /// </summary>
    public Mesh Translate(Vector3 offset)
    {
        var newTriangles = _triangles.Select(t => new Triangle(
            t.V1 + offset,
            t.V2 + offset,
            t.V3 + offset
        ));
        return new Mesh(newTriangles);
    }

    /// <summary>
    /// Centers the mesh at the origin (XY plane).
    /// </summary>
    public Mesh CenterAtOrigin()
    {
        var (min, max) = GetBoundingBox();
        var centerX = (min.X + max.X) / 2;
        var centerY = (min.Y + max.Y) / 2;
        return Translate(new Vector3(-centerX, -centerY, 0));
    }

    public void Clear()
    {
        _triangles.Clear();
    }
}
