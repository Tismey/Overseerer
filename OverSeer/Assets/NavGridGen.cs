using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct NodeGrid
{
    public float[] heights;              // Liste des hauteurs détectées
    public bool[][] connections;         // connections[h][8]
    public bool[][] cover;               // cover[h][8]
    public bool[] isCorner;
    public bool[] isInsideGeometry; // isCorner[h]
}


public class NavGridGen : MonoBehaviour
{
    [Header("Grid Bounds")]
    public Transform pointA;
    public Transform pointB;

    [Header("Grid Size")]
    public int gridX = 20;
    public int gridY = 20;

    [Header("Layers to Raycast")]
    public LayerMask geometryMask;

    public NodeGrid[,] grid;

    private float cellSizeX;
    private float cellSizeZ;

    // ----------------------------------------------------------------------
    void Start()
    {
        GenerateGrid();
    }

    // ----------------------------------------------------------------------
    public void GenerateGrid()
    {
        grid = new NodeGrid[gridX, gridY];

        Vector3 worldMin = pointA.position;
        Vector3 worldMax = pointB.position;

        cellSizeX = (worldMax.x - worldMin.x) / gridX;
        cellSizeZ = (worldMax.z - worldMin.z) / gridY;

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                Vector3 center = GridToWorld(i, j);
                Vector3 rayOrigin = new Vector3(center.x, worldMax.y + 50f, center.z);

                float rayLength = Mathf.Abs(worldMax.y - worldMin.y) + 200f;
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, rayLength, geometryMask);

                float[] heights = new float[hits.Length];
                for (int h = 0; h < hits.Length; h++)
                    heights[h] = hits[h].point.y;

                grid[i, j] = new NodeGrid { heights = heights };
            }
        }

        ComputeConnections();
        ComputeCover();
    }

    // ----------------------------------------------------------------------
    bool IsInsideGeometry(Vector3 pos)
    {
        return Physics.OverlapSphere(pos + Vector3.up * 1.1f, 1f, geometryMask).Length > 0;
    }

    // ----------------------------------------------------------------------
    int FindBestHeightIndex(NodeGrid neighbor, float targetHeight, Vector3 basePos)
    {
        float[] heights = neighbor.heights;
        int H = heights.Length;

        List<(int h, float diff)> list = new List<(int, float)>(H);

        for (int i = 0; i < H; i++)
            list.Add((i, Mathf.Abs(heights[i] - targetHeight)));

        list.Sort((a, b) => a.diff.CompareTo(b.diff));

        foreach (var c in list)
        {
            Vector3 p = basePos;
            p.y = heights[c.h];

            if (!IsInsideGeometry(p))
                return c.h;
        }

        return -1;
    }

    // ----------------------------------------------------------------------
    public void ComputeConnections(float maxSlope = 35f)
    {
        if (grid == null) return;

        Vector2Int[] offsets = new Vector2Int[]
        {
            new Vector2Int(-1, -1),
            new Vector2Int( 0, -1),
            new Vector2Int( 1, -1),
            new Vector2Int(-1,  0),
            new Vector2Int( 1,  0),
            new Vector2Int(-1,  1),
            new Vector2Int( 0,  1),
            new Vector2Int( 1,  1)
        };

        Vector3[] directions3D = new Vector3[]
        {
            new Vector3(-1,0,-1).normalized,
            new Vector3( 0,0,-1),
            new Vector3( 1,0,-1).normalized,
            new Vector3(-1,0, 0),
            new Vector3( 1,0, 0),
            new Vector3(-1,0, 1).normalized,
            new Vector3( 0,0, 1),
            new Vector3( 1,0, 1).normalized
        };

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                NodeGrid node = grid[i, j];

                int H = (node.heights == null ? 0 : node.heights.Length);

                node.connections = new bool[H][];
                node.cover = new bool[H][];
                node.isCorner = new bool[H];
                node.isInsideGeometry = new bool[H];

                if (H == 0)
                {
                    grid[i, j] = node;
                    continue;
                }

                // Création + init à TRUE des connexions
                for (int h = 0; h < H; h++)
                {
                    node.connections[h] = new bool[8];
                    node.cover[h] = new bool[8];
                    node.isCorner[h] = false;

                    for (int d = 0; d < 8; d++)
                        node.connections[h][d] = true; // nouvelle règle
                }

                // ---- Loop sur chaque height ----
                for (int hA = 0; hA < H; hA++)
                {
                    float heightA = node.heights[hA];
                    Vector3 posA = GridToWorld(i, j);
                    posA.y = heightA;

                    // Point intérieur → tout bloquer
                    if (IsInsideGeometry(posA))
                    {
                        for (int d = 0; d < 8; d++)
                        {
                            node.connections[hA][d] = false;
                            node.cover[hA][d] = false;
                            
                        }
                        node.isInsideGeometry[hA] = true;
                        continue;
                    }
                    node.isInsideGeometry[hA] = false;
                    // Sinon on teste les connexions
                    for (int d = 0; d < 8; d++)
                    {
                        int nx = i + offsets[d].x;
                        int ny = j + offsets[d].y;

                        if (nx < 0 || nx >= gridX || ny < 0 || ny >= gridY)
                        {
                            node.connections[hA][d] = false;
                            continue;
                        }

                        NodeGrid neighbor = grid[nx, ny];
                        if (neighbor.heights == null || neighbor.heights.Length == 0)
                        {
                            node.connections[hA][d] = false;
                            continue;
                        }

                        Vector3 neighborBase = GridToWorld(nx, ny);

                        // Trouve une hauteur valide
                        int hB = FindBestHeightIndex(neighbor, heightA, neighborBase);
                        if (hB < 0)
                        {
                            node.connections[hA][d] = false;
                            continue;
                        }

                        float heightB = neighbor.heights[hB];
                        Vector3 posB = neighborBase;
                        posB.y = heightB;

                        bool blocked = Physics.Linecast(posA + Vector3.up * 5f,
                                                        posB + Vector3.up * 5f,
                                                        geometryMask);

                        if (blocked)
                        {
                            node.connections[hA][d] = false;
                            continue;
                        }

                        float deltaY = Mathf.Abs(heightA - heightB);
                        float dist = Vector2.Distance(new Vector2(posA.x, posA.z),
                                                      new Vector2(posB.x, posB.z));
                        float slope = Mathf.Atan(deltaY / dist) * Mathf.Rad2Deg;

                        if (slope > maxSlope)
                        {
                            node.connections[hA][d] = false;
                            continue;
                        }

                    }

                }

                grid[i, j] = node;
            }
        }
    }


    public void ComputeCover()
    {
        if (grid == null) return;

        Vector2Int[] offsets = new Vector2Int[]
        {
        new Vector2Int(-1, -1),
        new Vector2Int( 0, -1),
        new Vector2Int( 1, -1),
        new Vector2Int(-1,  0),
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  1),
        new Vector2Int( 0,  1),
        new Vector2Int( 1,  1)
        };

        Vector3[] directions3D = new Vector3[]
        {
        new Vector3(-1,0,-1).normalized,
        new Vector3( 0,0,-1),
        new Vector3( 1,0,-1).normalized,
        new Vector3(-1,0, 0),
        new Vector3( 1,0, 0),
        new Vector3(-1,0, 1).normalized,
        new Vector3( 0,0, 1),
        new Vector3( 1,0, 1).normalized
        };

        // 1) Calcul du cover pour chaque noeud et chaque hauteur
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                NodeGrid node = grid[i, j];
                int H = (node.heights == null ? 0 : node.heights.Length);
                if (H == 0)
                {
                    grid[i, j] = node;
                    continue;
                }

                Vector3 basePos = GridToWorld(i, j);

                if (node.cover == null || node.cover.Length != H)
                    node.cover = new bool[H][];
                if (node.isCorner == null || node.isCorner.Length != H)
                    node.isCorner = new bool[H]; // Reset ici aussi, on recalculera plus tard

                for (int hA = 0; hA < H; hA++)
                {
                    if (node.cover[hA] == null || node.cover[hA].Length != 8)
                        node.cover[hA] = new bool[8];

                    float heightA = node.heights[hA];
                    Vector3 posA = basePos;
                    posA.y = heightA;
                    Vector3 coverOrigin = posA + Vector3.up * 2.5f;

                    for (int d = 0; d < 8; d++)
                    {
                        int nx = i + offsets[d].x;
                        int ny = j + offsets[d].y;

                        if (nx < 0 || nx >= gridX || ny < 0 || ny >= gridY)
                        {
                            node.cover[hA][d] = false;
                            continue;
                        }

                        Vector3 neighborPos = GridToWorld(nx, ny);
                        neighborPos.y = posA.y;

                        float dist = Vector3.Distance(
                            new Vector3(posA.x, 0, posA.z),
                            new Vector3(neighborPos.x, 0, neighborPos.z)
                        );

                        node.cover[hA][d] = Physics.Raycast(
                            coverOrigin,
                            directions3D[d],
                            dist,
                            geometryMask
                        );
                    }
                }
                grid[i, j] = node;
            }
        }

        // 2) Détection des coins en fonction du cover calculé
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                NodeGrid node = grid[i, j];
                int H = (node.heights == null ? 0 : node.heights.Length);
                if (H == 0)
                {
                    grid[i, j] = node;
                    continue;
                }

                for (int hA = 0; hA < H; hA++)
                {
                    int coverCount = 0;
                    for (int d = 0; d < 8; d++)
                        if (node.cover[hA][d]) coverCount++;

                    node.isCorner[hA] = false;

                    if (coverCount == 1)
                    {
                        for (int d = 0; d < 8; d++)
                        {
                            int nx = i + offsets[d].x;
                            int ny = j + offsets[d].y;

                            if (nx < 0 || nx >= gridX || ny < 0 || ny >= gridY)
                                continue;

                            NodeGrid neighbor = grid[nx, ny];
                            if (neighbor.cover == null)
                                continue;

                            foreach (var cList in neighbor.cover)
                            {
                                int neighborCoverCount = 0;
                                foreach (bool c in cList)
                                    if (c) neighborCoverCount++;

                                if (neighborCoverCount > 1)
                                {
                                    node.isCorner[hA] = true;
                                    break;
                                }
                            }

                            if (node.isCorner[hA])
                                break;
                        }
                    }
                }
                grid[i, j] = node;
            }
        }
    }


    // ----------------------------------------------------------------------
    public Vector3 GridToWorld(int i, int j)
    {
        Vector3 min = pointA.position;

        float px = min.x + i * cellSizeX + cellSizeX / 2f;
        float pz = min.z + j * cellSizeZ + cellSizeZ / 2f;

        return new Vector3(px, 0f, pz);
    }

    // ----------------------------------------------------------------------
    void OnDrawGizmos()
    {
        if (grid == null) return;

        Vector2Int[] offsets = new Vector2Int[]
        {
            new Vector2Int(-1,-1),
            new Vector2Int( 0,-1),
            new Vector2Int( 1,-1),
            new Vector2Int(-1, 0),
            new Vector2Int( 1, 0),
            new Vector2Int(-1, 1),
            new Vector2Int( 0, 1),
            new Vector2Int( 1, 1)
        };

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                NodeGrid node = grid[i, j];
                Vector3 basePos = GridToWorld(i, j);

                if (node.heights == null) continue;

                int H = node.heights.Length;

                for (int h = 0; h < H; h++)
                {
                    Vector3 pos = basePos;
                    pos.y = node.heights[h];

                    // POINTS VERTS
                    Gizmos.color = Color.green;
                    if (node.isInsideGeometry[h]) { Gizmos.color = Color.red; Gizmos.DrawSphere(pos, 0.6f); }
                    else { Gizmos.color = Color.green; Gizmos.DrawSphere(pos, 0.6f); }

                    // COVER
                    for (int d = 0; d < 8; d++)
                    {
                        if (node.cover[h][d])
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawSphere(pos + new Vector3(offsets[d].x, 0, offsets[d].y), 0.4f);
                        }
                    }

                    // CORNER
                    if (node.isCorner[h])
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(pos + Vector3.up * 0.5f, 1.2f);
                    }

                    // CONNEXIONS BLOQUÉES
                    for (int d = 0; d < 8; d++)
                    {

                        if (!node.connections[h][d])
                        {
                            int nx = i + offsets[d].x;
                            int ny = j + offsets[d].y;

                            if (nx < 0 || nx >= gridX || ny < 0 || ny >= gridY) continue;

                            NodeGrid nb = grid[nx, ny];
                            if (nb.heights == null) continue;

                            for(int hb = 0; hb < nb.heights.Length; hb++)
                            {
                                Vector3 posB = GridToWorld(nx, ny);
                                posB.y = nb.heights[hb];

                                Gizmos.color = Color.red;
                                Gizmos.DrawLine(pos, posB);
                            }
                            
                        }
                    }
                }
            }
        }
    }
}
