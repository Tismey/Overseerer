using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CoverType : ushort
{
    None = 0,
    Low = 1,
    High = 2
}

public class Node
{
    public Vector3 worldPos;
    public float elevation;
    public bool occupied;
    public ushort coverMask; // 8 directions x 2 bits
    public bool isCorner;    // True si c’est un coin
}

public class NodeGeneration : MonoBehaviour
{
    [Header("Zone de génération")]
    public Transform cornerA;
    public Transform cornerB;
    public int width = 50;
    public int depth = 50;

    [Header("Détection de surface et obstacles")]
    public LayerMask groundMask;
    public LayerMask obstructionMask;

    [Header("Paramètres de couverture (par relief)")]
    public float lowCoverThreshold = 0.6f;    // Diff élévation min pour couvert bas
    public float highCoverThreshold = 1.4f;   // Diff élévation min pour couvert haut

    [Header("Paramètres de raycast de couverture")]
    public float lowRaycastHeight = 0.5f;     // Hauteur du raycast pour couvert bas
    public float highRaycastHeight = 1.5f;    // Hauteur du raycast pour couvert haut

    [Header("Check obstacle pour occupation")]
    public Vector3 occupancyCheckBoxSize = new Vector3(0.25f, 0.5f, 0.25f);

    private Node[,] nodes;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        nodes = new Node[width, depth];
        Vector3 size = cornerB.position - cornerA.position;
        Vector3 origin = cornerA.position;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                nodes[x, z] = new Node();
                float fx = (x + 0.5f) / width;
                float fz = (z + 0.5f) / depth;
                Vector3 centre = origin + new Vector3(fx * size.x, 0, fz * size.z);

                RaycastHit hit;
                if (Physics.Raycast(centre + Vector3.up * 100f, Vector3.down, out hit, 200f, groundMask))
                {
                    nodes[x, z].worldPos = hit.point;
                    nodes[x, z].elevation = hit.point.y;

                    // Vérifie si le node est dans un obstacle
                    if (Physics.CheckBox(hit.point + Vector3.up * occupancyCheckBoxSize.y * 0.5f,
                        occupancyCheckBoxSize * 0.5f, Quaternion.identity, obstructionMask))
                    {
                        nodes[x, z].occupied = true;
                    }
                    else
                    {
                        nodes[x, z].occupied = false;
                    }
                }
                else
                {
                    // Pas de sol détecté, on marque comme occupé
                    nodes[x, z].worldPos = centre;
                    nodes[x, z].elevation = centre.y;
                    nodes[x, z].occupied = true;
                }
            }
        }

        ComputeCoverMasks();
    }

    void ComputeCoverMasks()
    {
        int[,] dirs = {
            {0,1}, {1,1}, {1,0}, {1,-1},
            {0,-1}, {-1,-1}, {-1,0}, {-1,1}
        };

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                Node node = nodes[x, z];
                ushort mask = 0;

                for (int i = 0; i < 8; i++)
                {
                    int nx = x + dirs[i, 0];
                    int nz = z + dirs[i, 1];
                    if (nx < 0 || nz < 0 || nx >= width || nz >= depth) continue;

                    Node neighbor = nodes[nx, nz];

                    CoverType cover = CoverType.None;

                    if (neighbor.occupied)
                    {
                        cover = CoverType.Low;
                    }
                    else
                    {
                        float delta = neighbor.elevation - node.elevation;

                        if (delta >= highCoverThreshold)
                        {
                            cover = CoverType.High;
                        }
                        else if (delta >= lowCoverThreshold)
                        {
                            cover = CoverType.Low;
                        }
                        else
                        {
                            // Raycasts bas et haut
                            Vector3 dir = (neighbor.worldPos - node.worldPos).normalized;
                            float dist = Vector3.Distance(node.worldPos, neighbor.worldPos);

                            Vector3 fromLow = node.worldPos + Vector3.up * lowRaycastHeight;
                            Vector3 fromHigh = node.worldPos + Vector3.up * highRaycastHeight;
                            Vector3 to = neighbor.worldPos + Vector3.up * lowRaycastHeight;

                            bool lowHit = Physics.Raycast(fromLow, dir, dist, obstructionMask);
                            bool highHit = Physics.Raycast(fromHigh, dir, dist, obstructionMask);

                            if (highHit) cover = CoverType.High;
                            else if (lowHit) cover = CoverType.Low;
                        }
                    }

                    // Stockage 2 bits par direction
                    mask |= (ushort)((int)cover << (i * 2));
                    int coverDirs = 0;
                    for (int t = 0; t < 8; t++)
                    {
                        CoverType c = (CoverType)((mask >> (i * 2)) & 0b11);
                        if (c != CoverType.None) coverDirs++;
                    }
                    node.isCorner = coverDirs <= 2 && coverDirs > 0;
                }

                node.coverMask = mask;
            }
        }
    }

    public Vector3 GridToWorld(int gx, int gz)
    {
        return nodes[gx, gz].worldPos;
    }

    public bool GridToWorldSafe(int gx, int gz, out Vector3 pos)
    {
        pos = Vector3.zero;
        if (gx >= 0 && gz >= 0 && gx < width && gz < depth)
        {
            pos = nodes[gx, gz].worldPos;
            return true;
        }
        return false;
    }

    public bool WorldToGrid(Vector3 world, out int gx, out int gz)
    {
        gx = gz = -1;
        Vector3 size = cornerB.position - cornerA.position;
        Vector3 local = world - cornerA.position;
        float fx = Mathf.InverseLerp(0, size.x, local.x);
        float fz = Mathf.InverseLerp(0, size.z, local.z);
        gx = Mathf.FloorToInt(fx * width);
        gz = Mathf.FloorToInt(fz * depth);
        return gx >= 0 && gz >= 0 && gx < width && gz < depth;
    }

    public CoverType GetCover(Node node, int directionIndex)
    {
        return (CoverType)((node.coverMask >> (directionIndex * 2)) & 0b11);
    }

    public Node[,] GetNodes() => nodes;

    void OnDrawGizmosSelected()
    {
        if (nodes == null) return;

        float cubeSize = 0.2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                Node node = nodes[x, z];
                Vector3 pos = node.worldPos + Vector3.up * 0.05f;

                // Couleur du node
                if (node.occupied) Gizmos.color = Color.red;
                else if (node.isCorner) Gizmos.color = Color.yellow;
                else Gizmos.color = Color.green;

                Gizmos.DrawCube(pos, Vector3.one * cubeSize);

                // Couverture : lignes vers voisins selon type
                for (int i = 0; i < 8; i++)
                {
                    CoverType cover = (CoverType)((node.coverMask >> (i * 2)) & 0b11);
                    if (cover == CoverType.None) continue;

                    int dx = 0, dz = 0;
                    switch (i)
                    {
                        case 0: dz = 1; break;   // N
                        case 1: dx = 1; dz = 1; break;  // NE
                        case 2: dx = 1; break;   // E
                        case 3: dx = 1; dz = -1; break; // SE
                        case 4: dz = -1; break;  // S
                        case 5: dx = -1; dz = -1; break;// SW
                        case 6: dx = -1; break;  // W
                        case 7: dx = -1; dz = 1; break; // NW
                    }

                    int nx = x + dx;
                    int nz = z + dz;
                    if (nx < 0 || nz < 0 || nx >= width || nz >= depth) continue;

                    Node neighbor = nodes[nx, nz];
                    Vector3 to = neighbor.worldPos + Vector3.up * 0.05f;

                    // Couleur selon type de couverture
                    Gizmos.color = (cover == CoverType.High) ? Color.cyan : Color.blue;
                    Gizmos.DrawLine(pos, to);
                }
            }
        }
    }
}




