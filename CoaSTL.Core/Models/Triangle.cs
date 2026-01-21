namespace CoaSTL.Core.Models;

/// <summary>
/// Represents a triangle in 3D space with three vertices and a computed normal.
/// </summary>
public sealed class Triangle
{
    public Vector3 V1 { get; }
    public Vector3 V2 { get; }
    public Vector3 V3 { get; }
    public Vector3 Normal { get; }

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        Normal = ComputeNormal();
    }

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        Normal = normal;
    }

    private Vector3 ComputeNormal()
    {
        var edge1 = V2 - V1;
        var edge2 = V3 - V1;
        return Vector3.Cross(edge1, edge2).Normalized();
    }

    /// <summary>
    /// Returns a new triangle with vertices in reversed order (flipped normal).
    /// </summary>
    public Triangle Flip() => new(V3, V2, V1);
}
