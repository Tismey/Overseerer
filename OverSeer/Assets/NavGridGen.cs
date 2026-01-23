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
    public Transform pointpA;
    public Transform pointB;

    public static Transform pointA;

    [Header("Grid Size")]
    public int setGridX = 20;
    public int setGridY = 20;


    public static int gridX;
    public static int gridY;

    [Header("Layers to Raycast")]
    public LayerMask geometryMask;

    public static NodeGrid[,] grid;

    private static float cellSizeX;
    private static float cellSizeZ;

    public static Vector2Int[] offsets = new Vector2Int[]
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

    // ----------------------------------------------------------------------
    void Start()
    {
        pointA = pointpA;
        gridX = setGridX;
        gridY = setGridY;
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
                        node.connections[h][d] = false; // nouvelle règle
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
                            continue;
                        }

                        NodeGrid neighbor = grid[nx, ny];
                        if (neighbor.heights == null || neighbor.heights.Length == 0)
                        {
                            continue;
                        }

                        Vector3 neighborBase = GridToWorld(nx, ny);

                        // Trouve une hauteur valide
                        int hB = FindBestHeightIndex(neighbor, heightA, neighborBase);
                        if (hB < 0)
                        {
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
                            continue;
                        }

                        float deltaY = Mathf.Abs(heightA - heightB);
                        float dist = Vector2.Distance(new Vector2(posA.x, posA.z),
                                                      new Vector2(posB.x, posB.z));
                        float slope = Mathf.Atan(deltaY / dist) * Mathf.Rad2Deg;

                        if (slope > maxSlope)
                        {
                            continue;
                        }

                        node.connections[hA][d] = true;

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
    public static Vector3 GridToWorld(int i, int j)
    {
        Vector3 min = pointA.position;

        float px = min.x + i * cellSizeX + cellSizeX / 2f;
        float pz = min.z + j * cellSizeZ + cellSizeZ / 2f;

        return new Vector3(px, 0f, pz);
    }

    // Convertit un point monde en indices grille
    public static bool WorldToGrid(Vector3 pos, out int i, out int j, out int h)
    {
        h = -1;

        // Convertit XZ → indices grille
        Vector3 min = pointA.position;

        i = Mathf.FloorToInt((pos.x - min.x) / cellSizeX);
        j = Mathf.FloorToInt((pos.z - min.z) / cellSizeZ);

        // Hors de la grille
        if (i < 0 || i >= gridX || j < 0 || j >= gridY)
            return false;

        // Le nœud correspondant
        NodeGrid node = grid[i, j];

        // Si aucune hauteur disponible → échec
        if (node.heights == null || node.heights.Length == 0)
            return false;

        // Étape : sélectionner la hauteur la plus proche du point monde
        float targetY = pos.y;
        float bestDist = Mathf.Infinity;

        for (int k = 0; k < node.heights.Length; k++)
        {
            float dh = Mathf.Abs(node.heights[k] - targetY);

            if (dh < bestDist)
            {
                bestDist = dh;
                h = k;
            }
        }

        return true;
    }

    public static int GetClosestDirectionIndex(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            return -1;

        dir.Normalize();

        float bestDot = float.MinValue;
        int bestIndex = -1;

        for (int i = 0; i < 8; i++)
        {
            Vector2 offset = NavGridGen.offsets[i];
            Vector3 offDir = new Vector3(offset.x, 0f, offset.y).normalized;

            float dot = Vector3.Dot(dir, offDir);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public static bool IsValid(int i , int j)
    {
        if (i < 0 || j < 0) return false;
        if (i >= gridX || j >= gridY) return false;
        return true;
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
