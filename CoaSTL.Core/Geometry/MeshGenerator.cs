using CoaSTL.Core.Models;

namespace CoaSTL.Core.Geometry;

/// <summary>
/// Generates 3D mesh from 2D profiles to create coaster geometry.
/// </summary>
public sealed class MeshGenerator
{
    /// <summary>
    /// Generates a coaster mesh from the given settings.
    /// </summary>
    public Mesh GenerateCoaster(CoasterSettings settings, float[,]? heightMap = null)
    {
        settings.Validate();
        var mesh = new Mesh();
        var profile = ShapeGenerator.GenerateProfile(settings);

        // Generate the base geometry
        GenerateBase(mesh, profile, settings);

        // Apply height map if provided
        if (heightMap != null)
        {
            GenerateTopSurfaceWithHeightMap(mesh, profile, settings, heightMap);
        }
        else
        {
            GenerateTopSurface(mesh, profile, settings.TotalHeight);
        }

        // Generate side walls
        GenerateSideWalls(mesh, profile, settings);

        // Add edge treatment
        if (settings.EdgeStyle != EdgeStyle.Flat)
        {
            ApplyEdgeTreatment(mesh, profile, settings);
        }

        // Add non-slip bottom if enabled
        if (settings.AddNonSlipBottom)
        {
            AddNonSlipPattern(mesh, profile, settings);
        }

        return mesh;
    }

    private void GenerateBase(Mesh mesh, List<(float X, float Y)> profile, CoasterSettings settings)
    {
        // Create bottom face using fan triangulation
        var center = new Vector3(0, 0, 0);

        for (int i = 0; i < profile.Count; i++)
        {
            var p1 = profile[i];
            var p2 = profile[(i + 1) % profile.Count];

            // Clockwise winding for bottom face (normal points down)
            mesh.AddTriangle(
                center,
                new Vector3(p2.X, p2.Y, 0),
                new Vector3(p1.X, p1.Y, 0)
            );
        }
    }

    private void GenerateTopSurface(Mesh mesh, List<(float X, float Y)> profile, float height)
    {
        // Create top face using fan triangulation
        var center = new Vector3(0, 0, height);

        for (int i = 0; i < profile.Count; i++)
        {
            var p1 = profile[i];
            var p2 = profile[(i + 1) % profile.Count];

            // Counter-clockwise winding for top face (normal points up)
            mesh.AddTriangle(
                center,
                new Vector3(p1.X, p1.Y, height),
                new Vector3(p2.X, p2.Y, height)
            );
        }
    }

    private void GenerateTopSurfaceWithHeightMap(Mesh mesh, List<(float X, float Y)> profile,
        CoasterSettings settings, float[,] heightMap)
    {
        var mapWidth = heightMap.GetLength(0);
        var mapHeight = heightMap.GetLength(1);

        // Get bounding box of profile
        var minX = profile.Min(p => p.X);
        var maxX = profile.Max(p => p.X);
        var minY = profile.Min(p => p.Y);
        var maxY = profile.Max(p => p.Y);
        var width = maxX - minX;
        var height = maxY - minY;

        // Create a grid of points over the profile
        var gridResolution = Math.Max(mapWidth, mapHeight);
        var cellSizeX = width / gridResolution;
        var cellSizeY = height / gridResolution;

        // Generate vertices for the height map surface
        var vertices = new Vector3[gridResolution + 1, gridResolution + 1];

        for (int iy = 0; iy <= gridResolution; iy++)
        {
            for (int ix = 0; ix <= gridResolution; ix++)
            {
                var x = minX + ix * cellSizeX;
                var y = minY + iy * cellSizeY;

                // Sample height from height map
                var mapX = (int)((float)ix / gridResolution * (mapWidth - 1));
                var mapY = (int)((float)iy / gridResolution * (mapHeight - 1));
                mapX = Math.Clamp(mapX, 0, mapWidth - 1);
                mapY = Math.Clamp(mapY, 0, mapHeight - 1);

                var normalizedHeight = heightMap[mapX, mapY];
                var reliefHeight = normalizedHeight * settings.ReliefDepth;

                if (settings.InvertRelief)
                {
                    reliefHeight = settings.ReliefDepth - reliefHeight;
                }

                var z = settings.BaseThickness + reliefHeight;

                // Check if point is inside profile, using ternary for clarity
                var vertexHeight = IsPointInProfile(x, y, profile) ? z : settings.BaseThickness;
                vertices[ix, iy] = new Vector3(x, y, vertexHeight);
            }
        }

        // Generate triangles from the grid
        for (int iy = 0; iy < gridResolution; iy++)
        {
            for (int ix = 0; ix < gridResolution; ix++)
            {
                var v00 = vertices[ix, iy];
                var v10 = vertices[ix + 1, iy];
                var v01 = vertices[ix, iy + 1];
                var v11 = vertices[ix + 1, iy + 1];

                // Add triangles based on centroid inclusion to avoid gaps at boundaries
                var tri1CenterX = (v00.X + v10.X + v11.X) / 3f;
                var tri1CenterY = (v00.Y + v10.Y + v11.Y) / 3f;
                if (IsPointInProfile(tri1CenterX, tri1CenterY, profile))
                {
                    mesh.AddTriangle(v00, v10, v11);
                }

                var tri2CenterX = (v00.X + v11.X + v01.X) / 3f;
                var tri2CenterY = (v00.Y + v11.Y + v01.Y) / 3f;
                if (IsPointInProfile(tri2CenterX, tri2CenterY, profile))
                {
                    mesh.AddTriangle(v00, v11, v01);
                }
            }
        }

        // Also generate the edge triangles connecting to the profile boundary
        GenerateEdgeSurfaceWithHeightMap(mesh, profile, settings, heightMap);
    }

    private void GenerateEdgeSurfaceWithHeightMap(Mesh mesh, List<(float X, float Y)> profile,
        CoasterSettings settings, float[,] heightMap)
    {
        var mapWidth = heightMap.GetLength(0);
        var mapHeight = heightMap.GetLength(1);

        var minX = profile.Min(p => p.X);
        var maxX = profile.Max(p => p.X);
        var minY = profile.Min(p => p.Y);
        var maxY = profile.Max(p => p.Y);
        var width = maxX - minX;
        var height = maxY - minY;

        // Create top edge connecting to profile boundary
        for (int i = 0; i < profile.Count; i++)
        {
            var p1 = profile[i];
            var p2 = profile[(i + 1) % profile.Count];

            // Sample heights at edge points
            var mapX1 = (int)((p1.X - minX) / width * (mapWidth - 1));
            var mapY1 = (int)((p1.Y - minY) / height * (mapHeight - 1));
            mapX1 = Math.Clamp(mapX1, 0, mapWidth - 1);
            mapY1 = Math.Clamp(mapY1, 0, mapHeight - 1);

            var mapX2 = (int)((p2.X - minX) / width * (mapWidth - 1));
            var mapY2 = (int)((p2.Y - minY) / height * (mapHeight - 1));
            mapX2 = Math.Clamp(mapX2, 0, mapWidth - 1);
            mapY2 = Math.Clamp(mapY2, 0, mapHeight - 1);

            var h1 = heightMap[mapX1, mapY1] * settings.ReliefDepth;
            var h2 = heightMap[mapX2, mapY2] * settings.ReliefDepth;

            if (settings.InvertRelief)
            {
                h1 = settings.ReliefDepth - h1;
                h2 = settings.ReliefDepth - h2;
            }

            // Note: z1, z2 and centerX, centerY were computed but not used in the original implementation.
            // Edge surface generation is handled by the main height map surface triangulation.
        }
    }

    private bool IsPointInProfile(float x, float y, List<(float X, float Y)> profile)
    {
        // Ray casting algorithm for point-in-polygon test
        int n = profile.Count;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var pi = profile[i];
            var pj = profile[j];

            if ((pi.Y > y) != (pj.Y > y) &&
                x < (pj.X - pi.X) * (y - pi.Y) / (pj.Y - pi.Y) + pi.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private void GenerateSideWalls(Mesh mesh, List<(float X, float Y)> profile, CoasterSettings settings)
    {
        var bottomZ = 0f;
        var topZ = settings.TotalHeight;

        for (int i = 0; i < profile.Count; i++)
        {
            var p1 = profile[i];
            var p2 = profile[(i + 1) % profile.Count];

            // Create a quad (two triangles) for each edge
            var v1Bottom = new Vector3(p1.X, p1.Y, bottomZ);
            var v2Bottom = new Vector3(p2.X, p2.Y, bottomZ);
            var v1Top = new Vector3(p1.X, p1.Y, topZ);
            var v2Top = new Vector3(p2.X, p2.Y, topZ);

            // Counter-clockwise winding for outward-facing normals
            mesh.AddTriangle(v1Bottom, v2Bottom, v2Top);
            mesh.AddTriangle(v1Bottom, v2Top, v1Top);
        }
    }

    private void ApplyEdgeTreatment(Mesh mesh, List<(float X, float Y)> profile, CoasterSettings settings)
    {
        // Edge treatment is simplified for MVP - just add basic edge geometry
        switch (settings.EdgeStyle)
        {
            case EdgeStyle.Beveled:
                // Bevel would modify the side wall geometry
                break;
            case EdgeStyle.Rounded:
                // Rounded edge would add curve segments
                break;
            case EdgeStyle.RaisedRim:
                // Add raised rim around edge
                AddRaisedRim(mesh, profile, settings);
                break;
        }
    }

    private void AddRaisedRim(Mesh mesh, List<(float X, float Y)> profile, CoasterSettings settings)
    {
        var rimHeight = Math.Min(2f, settings.TotalHeight - settings.BaseThickness);
        var rimWidth = 2f;
        var topZ = settings.TotalHeight;

        // Generate inner profile (offset inward)
        var innerProfile = OffsetProfile(profile, -rimWidth);

        // Create rim top surface
        for (int i = 0; i < profile.Count; i++)
        {
            var outerP1 = profile[i];
            var outerP2 = profile[(i + 1) % profile.Count];
            var innerP1 = innerProfile[i];
            var innerP2 = innerProfile[(i + 1) % innerProfile.Count];

            // Create quad between inner and outer edges at rim height
            mesh.AddQuad(
                new Vector3(innerP1.X, innerP1.Y, topZ),
                new Vector3(outerP1.X, outerP1.Y, topZ + rimHeight),
                new Vector3(outerP2.X, outerP2.Y, topZ + rimHeight),
                new Vector3(innerP2.X, innerP2.Y, topZ)
            );
        }
    }

    private List<(float X, float Y)> OffsetProfile(List<(float X, float Y)> profile, float offset)
    {
        var result = new List<(float X, float Y)>();
        var n = profile.Count;

        for (int i = 0; i < n; i++)
        {
            var prev = profile[(i - 1 + n) % n];
            var curr = profile[i];
            var next = profile[(i + 1) % n];

            // Calculate normals for adjacent edges
            var edge1 = (curr.X - prev.X, curr.Y - prev.Y);
            var edge2 = (next.X - curr.X, next.Y - curr.Y);

            // Perpendicular normals (pointing inward for negative offset)
            var n1 = Normalize((-edge1.Item2, edge1.Item1));
            var n2 = Normalize((-edge2.Item2, edge2.Item1));

            // Average normal at vertex
            var avgNormal = Normalize((n1.X + n2.X, n1.Y + n2.Y));

            result.Add((curr.X + avgNormal.X * offset, curr.Y + avgNormal.Y * offset));
        }

        return result;
    }

    private (float X, float Y) Normalize((float X, float Y) v)
    {
        var len = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        return len > 0 ? (v.X / len, v.Y / len) : (0, 0);
    }

    private void AddNonSlipPattern(Mesh mesh, List<(float X, float Y)> profile, CoasterSettings settings)
    {
        // Add small dots on the bottom surface for grip
        var dotRadius = 1f;
        var dotHeight = 0.5f;
        var spacing = 15f;

        var minX = profile.Min(p => p.X);
        var maxX = profile.Max(p => p.X);
        var minY = profile.Min(p => p.Y);
        var maxY = profile.Max(p => p.Y);

        for (float x = minX + spacing; x < maxX - spacing; x += spacing)
        {
            for (float y = minY + spacing; y < maxY - spacing; y += spacing)
            {
                if (IsPointInProfile(x, y, profile))
                {
                    AddDot(mesh, x, y, dotRadius, dotHeight);
                }
            }
        }
    }

    private void AddDot(Mesh mesh, float x, float y, float radius, float height)
    {
        // Create a small cylinder (simplified as cone for fewer triangles)
        const int segments = 8;
        var bottom = new Vector3(x, y, 0);
        var top = new Vector3(x, y, -height); // Extends below the base

        for (int i = 0; i < segments; i++)
        {
            var angle1 = 2 * MathF.PI * i / segments;
            var angle2 = 2 * MathF.PI * (i + 1) / segments;

            var p1 = new Vector3(x + radius * MathF.Cos(angle1), y + radius * MathF.Sin(angle1), 0);
            var p2 = new Vector3(x + radius * MathF.Cos(angle2), y + radius * MathF.Sin(angle2), 0);

            // Side face
            mesh.AddTriangle(bottom, p1, p2);

            // Bottom cap
            mesh.AddTriangle(top, p2, p1);
        }
    }
}
