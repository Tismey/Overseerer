using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NavMeshGenerator : MonoBehaviour
{
    public LayerMask walkableLayer;      // LayerMask for walkable objects
    public LayerMask notWalkableLayer;   // LayerMask for not-walkable objects
    public float maxSlope = 45f;         // Maximum slope angle (for handling slopes)
    public float polygonSize = 1f;       // The size of the polygons to generate

    private List<NavMeshPolygon> navMeshPolygons = new List<NavMeshPolygon>();

    void Start()
    {
        GenerateNavMesh();  // Generate the NavMesh when the scene starts
        Debug.Log("NavMesh polygons count: " + navMeshPolygons.Count);  // Print the number of polygons generated
    }

    void GenerateNavMesh()
    {
        // Find all colliders in the walkable layer
        Collider[] colliders = FindObjectsOfType<Collider>();

        foreach (Collider collider in colliders)
        {
            if (IsInLayerMask(collider.gameObject, walkableLayer))
            {
                // Get surface information (position, normal, size)
                GeneratePolygonsFromCollider(collider);
            }
        }

        ConnectPolygons();
    }

    bool IsInLayerMask(GameObject obj, LayerMask layer)
    {
        return ((layer.value & (1 << obj.layer)) > 0);
    }

    void GeneratePolygonsFromCollider(Collider collider)
    {
        // Determine the bounds and surface of the walkable area
        MeshCollider meshCollider = collider as MeshCollider;
        BoxCollider boxCollider = collider as BoxCollider;

        if (meshCollider != null)
        {
            GeneratePolygonsFromMeshCollider(meshCollider);
        }
        else if (boxCollider != null)
        {
            GeneratePolygonsFromBoxCollider(boxCollider);
        }

        // Now check for obstacles that intersect with the walkable polygons
        HandleObstructions(collider);
    }

    void HandleObstructions(Collider walkableCollider)
    {
        // Find all colliders in the not-walkable layer
        Collider[] notWalkableColliders = Physics.OverlapBox(
            walkableCollider.bounds.center,
            walkableCollider.bounds.extents,
            walkableCollider.transform.rotation,
            notWalkableLayer
        );

        foreach (Collider obstacle in notWalkableColliders)
        {
            SubtractObstacleFromPolygons(obstacle);
        }
    }

    void SubtractObstacleFromPolygons(Collider obstacle)
    {
        // Iterate over all existing navMeshPolygons and check for intersections with the obstacle
        List<NavMeshPolygon> updatedPolygons = new List<NavMeshPolygon>();

        foreach (NavMeshPolygon polygon in navMeshPolygons)
        {
            // Check if the polygon overlaps with the obstacle's bounds
            if (PolygonIntersectsWithObstacle(polygon, obstacle))
            {
                // Split or cut the polygon based on the obstacle's shape and position
                List<NavMeshPolygon> newPolygons = SplitPolygon(polygon, obstacle);
                updatedPolygons.AddRange(newPolygons);  // Add the new sub-polygons
            }
            else
            {
                updatedPolygons.Add(polygon);  // No intersection, keep the original polygon
            }
        }

        navMeshPolygons = updatedPolygons;  // Replace old polygons with updated list
    }

    bool PolygonIntersectsWithObstacle(NavMeshPolygon polygon, Collider obstacle)
    {
        // You can use polygon center and size to check if it overlaps with the obstacle's bounding box
        return obstacle.bounds.Intersects(new Bounds(polygon.position, new Vector3(polygon.width, 1f, polygon.length)));
    }

    List<NavMeshPolygon> SplitPolygon(NavMeshPolygon polygon, Collider obstacle)
    {
        // This method will depend on how complex you want the split to be.
        // A basic version can split the polygon into smaller chunks that avoid the obstacle.

        // For simplicity, let's assume we cut the polygon into 2 based on the obstacle's size
        List<NavMeshPolygon> result = new List<NavMeshPolygon>();

        // Calculate the new sizes/positions of the sub-polygons
        Vector3 obstaclePos = obstacle.bounds.center;
        Vector3 obstacleSize = obstacle.bounds.extents;

        // Create two new sub-polygons on either side of the obstacle
        // This is a simplified example—depending on your needs, this could be more complex
        NavMeshPolygon leftPolygon = new NavMeshPolygon(
            polygon.position - new Vector3(obstacleSize.x, 0, 0),
            polygon.normal,
            polygon.width / 2,
            polygon.length
        );

        NavMeshPolygon rightPolygon = new NavMeshPolygon(
            polygon.position + new Vector3(obstacleSize.x, 0, 0),
            polygon.normal,
            polygon.width / 2,
            polygon.length
        );

        result.Add(leftPolygon);
        result.Add(rightPolygon);

        return result;
    }

    void GeneratePolygonsFromMeshCollider(MeshCollider meshCollider)
    {
        // Extract mesh data
        Mesh mesh = meshCollider.sharedMesh;
        Transform transform = meshCollider.transform;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = transform.TransformPoint(vertices[triangles[i]]);
            Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 v3 = transform.TransformPoint(vertices[triangles[i + 2]]);

            // Calculate the polygon normal
            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            // Check if the slope is walkable
            if (Vector3.Angle(normal, Vector3.up) <= maxSlope)
            {
                // Calculate polygon center and size
                Vector3 center = (v1 + v2 + v3) / 3f;
                float polygonLength = Vector3.Distance(v1, v2);
                float polygonWidth = Vector3.Distance(v2, v3);

                // Create a new NavMeshPolygon
                NavMeshPolygon polygon = new NavMeshPolygon(center, normal, polygonLength, polygonWidth);
                navMeshPolygons.Add(polygon);
            }
        }
    }

    void GeneratePolygonsFromBoxCollider(BoxCollider boxCollider)
    {
        Transform transform = boxCollider.transform;
        Vector3 center = transform.TransformPoint(boxCollider.center);
        Vector3 size = Vector3.Scale(boxCollider.size, transform.localScale);
        Vector3 normal = transform.up;

        // Check if the surface is within the max slope
        if (Vector3.Angle(normal, Vector3.up) <= maxSlope)
        {
            // Create polygon using the size of the box collider
            NavMeshPolygon polygon = new NavMeshPolygon(center, normal, size.x, size.z);
            navMeshPolygons.Add(polygon);
        }
    }

    void ConnectPolygons()
    {
        foreach (NavMeshPolygon polygonA in navMeshPolygons)
        {
            foreach (NavMeshPolygon polygonB in navMeshPolygons)
            {
                if (polygonA == polygonB) continue;

                // If polygons are close enough to be neighbors, connect them
                if (Vector3.Distance(polygonA.position, polygonB.position) < polygonSize * 1.5f)
                {
                    polygonA.AddNeighbor(polygonB);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (navMeshPolygons != null)
        {
            foreach (NavMeshPolygon polygon in navMeshPolygons)
            {
                // Draw the main polygon as a filled cube for visibility
                Gizmos.color = Color.green;
                Gizmos.DrawCube(polygon.position, new Vector3(polygon.width, 0.1f, polygon.length));

                // Calculate the edges of the polygon based on width and length
                Vector3 halfWidth = polygon.normal * (polygon.width /2 );
                Vector3 halfLength = polygon.normal * (polygon.length /2 );

                // Calculate corners based on half width and half length
                Vector3 corner1 = polygon.position + halfWidth + halfLength; // Top right
                Vector3 corner2 = polygon.position + halfWidth - halfLength; // Bottom right
                Vector3 corner3 = polygon.position - halfWidth - halfLength; // Bottom left
                Vector3 corner4 = polygon.position - halfWidth + halfLength; // Top left

                // Draw edges between corners
                Gizmos.color = Color.blue; // Edge color
                Gizmos.DrawLine(corner1, corner2); // Right edge
                Gizmos.DrawLine(corner2, corner3); // Bottom edge
                Gizmos.DrawLine(corner3, corner4); // Left edge
                Gizmos.DrawLine(corner4, corner1); // Top edge

                // Draw connections between neighboring polygons
                Gizmos.color = Color.red;
                foreach (NavMeshPolygon neighbor in polygon.neighbors)
                {
                    Gizmos.DrawLine(polygon.position, neighbor.position); // Draw a line connecting polygons
                }
            }
        }

        // Draw obstacles in red
        Collider[] obstacles = Physics.OverlapBox(transform.position, transform.lossyScale / 2, Quaternion.identity, notWalkableLayer);
        Gizmos.color = Color.red;
        foreach (Collider obstacle in obstacles)
        {
            Gizmos.DrawWireCube(obstacle.bounds.center, obstacle.bounds.size);
        }
    }



}

// Helper class for NavMeshPolygon
public class NavMeshPolygon
{
    public Vector3 position;
    public Vector3 normal;
    public float width;
    public float length;
    public List<NavMeshPolygon> neighbors;

    public NavMeshPolygon(Vector3 position, Vector3 normal, float width, float length)
    {
        this.position = position;
        this.normal = normal;
        this.width = width;
        this.length = length;
        neighbors = new List<NavMeshPolygon>();
    }

    public void AddNeighbor(NavMeshPolygon neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
        }
    }
}

