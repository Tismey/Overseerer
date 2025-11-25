using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct NodeGrid
{
    public float[] heights;       // hauteurs détectées par RaycastAll

    // Liaisons (connexions) walkables vers voisins 8 directions
    // Indices : 0=NW,1=N,2=NE,3=W,4=E,5=SW,6=S,7=SE
    public bool[] connections;

    // Couverture dans les 8 directions (same indexing)
    public bool[] cover;

    // Coin (exactement 2 directions adjacentes couvertes)
    public bool isCorner;
}

public class NavGridGen : MonoBehaviour
{
    [Header("Grid Bounds")]
    public Transform pointA; // coin bas-gauche
    public Transform pointB; // coin haut-droit

    [Header("Grid Size")]
    public int gridX = 20; // colonnes
    public int gridY = 20; // lignes

    [Header("Layers to Raycast")]
    public LayerMask geometryMask;

    public NodeGrid[,] grid;

    private float cellSizeX;
    private float cellSizeZ;

    void Start()
    {
        GenerateGrid();
    }

    //----------------------------------------------------------------------
    // GENERATION DE LA GRILLE
    //----------------------------------------------------------------------
    public void GenerateGrid()
    {
        grid = new NodeGrid[gridX, gridY];

        Vector3 worldMin = pointA.position;
        Vector3 worldMax = pointB.position;

        // Taille d'une cellule
        cellSizeX = (worldMax.x - worldMin.x) / gridX;
        cellSizeZ = (worldMax.z - worldMin.z) / gridY;

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                // Position centrale de la cellule (en X/Z)
                Vector3 center = GridToWorld(i, j);
                Vector3 rayOrigin = new Vector3(center.x, worldMax.y + 50f, center.z);

                // RaycastAll vertical
                float rayLength = Mathf.Abs(worldMax.y - worldMin.y) + 200f;
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, rayLength, geometryMask);

                // Stockage des hauteurs
                float[] heights = new float[hits.Length];
                for (int h = 0; h < hits.Length; h++)
                    heights[h] = hits[h].point.y;

                grid[i, j] = new NodeGrid { heights = heights };
            }
        }

        ComputeConnectionsAndCover();

        Debug.Log($"Grid generated: {gridX} x {gridY}");
    }


    public void ComputeConnectionsAndCover(float maxSlope = 35f)
    {
        if (grid == null) return;

        // Directions 8 (NW, N, NE, W, E, SW, S, SE)
        Vector2Int[] offsets = new Vector2Int[]
        {
        new Vector2Int(-1, -1), // NW
        new Vector2Int( 0, -1), // N
        new Vector2Int( 1, -1), // NE
        new Vector2Int(-1,  0), // W
        new Vector2Int( 1,  0), // E
        new Vector2Int(-1,  1), // SW
        new Vector2Int( 0,  1), // S
        new Vector2Int( 1,  1)  // SE
        };

        // Directions 3D normalisées pour raycast horizontal
        Vector3[] directions3D = new Vector3[]
        {
        new Vector3(-1, 0, -1).normalized,  // NW
        new Vector3(0, 0, -1),               // N
        new Vector3(1, 0, -1).normalized,   // NE
        new Vector3(-1, 0, 0),               // W
        new Vector3(1, 0, 0),                // E
        new Vector3(-1, 0, 1).normalized,   // SW
        new Vector3(0, 0, 1),                // S
        new Vector3(1, 0, 1).normalized     // SE
        };

        // Distance de rayon : diagonale d'une cellule
        float rayDistance = Mathf.Sqrt(cellSizeX * cellSizeX + cellSizeZ * cellSizeZ);

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                NodeGrid node = grid[i, j];

                // Initialisation des tableaux
                node.connections = new bool[8];
                node.cover = new bool[8];
                node.isCorner = false;

                // Si pas de hauteur => on skip ce node
                if (node.heights == null || node.heights.Length == 0)
                {
                    grid[i, j] = node;
                    continue;
                }

                // Hauteur principale = max des hauteurs détectées
                float heightA = float.MinValue;
                foreach (float h in node.heights)
                    if (h > heightA) heightA = h;

                // Position world du node à hauteur principale
                Vector3 posA = GridToWorld(i, j);
                posA.y = heightA;

                for (int d = 0; d < 8; d++)
                {
                    int nx = i + offsets[d].x;
                    int ny = j + offsets[d].y;

                    if (nx < 0 || nx >= gridX || ny < 0 || ny >= gridY)
                    {
                        node.connections[d] = false;
                        node.cover[d] = false;
                        continue;
                    }

                    NodeGrid neighbor = grid[nx, ny];

                    // Si voisin pas de hauteur => pas de connexion
                    if (neighbor.heights == null || neighbor.heights.Length == 0)
                    {
                        node.connections[d] = false;
                        node.cover[d] = false;
                        continue;
                    }

                    // Hauteur principale voisin
                    float heightB = float.MinValue;
                    foreach (float h in neighbor.heights)
                        if (h > heightB) heightB = h;

                    Vector3 posB = GridToWorld(nx, ny);
                    posB.y = heightB;

                    // --- Test de connexion ---

                    // 1. Linecast entre A et B (levé de 0.1f pour éviter collision avec sol)
                    bool blocked = Physics.Linecast(posA + Vector3.up * 5f, posB + Vector3.up * 5f, geometryMask);

                    if (blocked)
                    {
                        node.connections[d] = false;
                    }
                    else
                    {
                        // 2. Calcul pente (en degrés)
                        float deltaY = Mathf.Abs(heightA - heightB);
                        float dist = Vector2.Distance(new Vector2(posA.x, posA.z), new Vector2(posB.x, posB.z));
                        float slope = Mathf.Atan(deltaY / dist) * Mathf.Rad2Deg;

                        node.connections[d] = slope <= maxSlope;
                    }

                    // --- Test de couvert ---

                    // Rayon horizontal depuis hauteur principale vers la direction 3D
                    Vector3 coverOrigin = posA + Vector3.up * 5f;

                    bool coverBlocked = Physics.Raycast(coverOrigin, directions3D[d], ((posA - posB)).magnitude, geometryMask);

                    node.cover[d] = coverBlocked;
                }

                // --- Calcul du coin ---

                // On compte les directions couvertes adjacentes (2 seulement)
                int coverCount = 0;
                foreach (bool c in node.cover)
                    if (c) coverCount++;

                bool isCorner = false;

                if (coverCount == 1)
                {
                    // Parcours voisins
                    foreach (Vector2Int offset in offsets)
                    {
                        int nx = i + offset.x;
                        int ny = j + offset.y;

                        if (nx < 0 || nx >= gridX || ny < 0 || ny >= gridY)
                            continue;

                        NodeGrid neighbor = grid[nx, ny];

                        if (neighbor.cover == null)
                            continue;

                        // Compter couvert du voisin
                        int neighborCoverCount = 0;
                        foreach (bool c in neighbor.cover)
                            if (c) neighborCoverCount++;

                        if (neighborCoverCount > 1)
                        {
                            isCorner = true;
                            break;
                        }
                    }
                }

                node.isCorner = isCorner;

                grid[i, j] = node;
            }
        }
    }


    //----------------------------------------------------------------------
    // CONVERSION GRID → WORLD
    //----------------------------------------------------------------------
    public Vector3 GridToWorld(int i, int j)
    {
        Vector3 worldMin = pointA.position;

        float px = worldMin.x + i * cellSizeX + cellSizeX / 2f;
        float pz = worldMin.z + j * cellSizeZ + cellSizeZ / 2f;

        return new Vector3(px, 0f, pz);
    }

    //----------------------------------------------------------------------
    // CONVERSION WORLD → GRID
    //----------------------------------------------------------------------
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 worldMin = pointA.position;

        int i = Mathf.FloorToInt((worldPos.x - worldMin.x) / cellSizeX);
        int j = Mathf.FloorToInt((worldPos.z - worldMin.z) / cellSizeZ);

        i = Mathf.Clamp(i, 0, gridX - 1);
        j = Mathf.Clamp(j, 0, gridY - 1);

        return new Vector2Int(i, j);
    }

    //----------------------------------------------------------------------
    // GIZMOS
    //----------------------------------------------------------------------
    void OnDrawGizmos()
    {
        if (grid == null) return;

        Vector2Int[] offsets = new Vector2Int[]
        {
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(-1,  0),                    new Vector2Int(1,  0),
        new Vector2Int(-1,  1), new Vector2Int(0,  1), new Vector2Int(1,  1)
        };

        for (int i = 0; i < gridX; i++)
            for (int j = 0; j < gridY; j++)
            {
                NodeGrid node = grid[i, j];
                Vector3 basePos = GridToWorld(i, j);

                if (node.heights == null) continue;

                // Dessin des surfaces (points verts)
                for (int h = 0; h < node.heights.Length; h++)
                {
                    float height = node.heights[h];
                    Vector3 pos = new Vector3(basePos.x, height, basePos.z);

                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(pos, 1f);
                }

                // Dessin des directions de couvert (points bleus)
                if (node.cover != null)
                {
                    for (int d = 0; d < 8; d++)
                    {
                        if (!node.cover[d]) continue;

                        int nx = i + offsets[d].x;
                        int ny = j + offsets[d].y;
                        if (nx < 0 || nx >= gridX || ny < 0 || ny >= gridY) continue;

                        float maxHeight = float.MinValue;
                        foreach (float height in node.heights)
                            if (height > maxHeight) maxHeight = height;

                        // Position point bleu légèrement décalé selon la direction de couvert
                        Vector3 offsetDir = new Vector3(offsets[d].x * cellSizeX, 0, offsets[d].y * cellSizeZ).normalized;
                        Vector3 coverPos = new Vector3(basePos.x, maxHeight + 0.5f, basePos.z) + offsetDir * 0.3f;

                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(coverPos, 0.5f);
                    }
                }

                // Point jaune pour coin
                if (node.isCorner)
                {
                    float maxHeight = float.MinValue;
                    foreach (float height in node.heights)
                        if (height > maxHeight) maxHeight = height;

                    Vector3 cornerPos = new Vector3(basePos.x, maxHeight + 0.2f, basePos.z);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(cornerPos, 5f);
                }

                // Lignes rouges pour connexions bloquées
                if (node.connections != null)
                {
                    for (int d = 0; d < 8; d++)
                    {
                        if (node.connections[d]) continue;

                        int nx = i + offsets[d].x;
                        int ny = j + offsets[d].y;
                        if (nx < 0 || nx >= gridX || ny < 0 || ny >= gridY) continue;

                        NodeGrid neighbor = grid[nx, ny];
                        if (neighbor.heights == null || neighbor.heights.Length == 0) continue;

                        float heightA = float.MinValue;
                        foreach (float h in node.heights)
                            if (h > heightA) heightA = h;
                        Vector3 posA = new Vector3(basePos.x, heightA, basePos.z);

                        Vector3 neighborBasePos = GridToWorld(nx, ny);
                        float heightB = float.MinValue;
                        foreach (float h in neighbor.heights)
                            if (h > heightB) heightB = h;
                        Vector3 posB = new Vector3(neighborBasePos.x, heightB, neighborBasePos.z);

                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(posA, posB);
                    }
                }
            }
    }

}





