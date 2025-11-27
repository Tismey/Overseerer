using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinder
{
    // -----------------------------------------------------
    // A* internal node
    // -----------------------------------------------------
    private class AStarNode
    {
        public int x, y, h;
        public float g;
        public float hCost;
        public float f => g + hCost;
        public AStarNode parent;

        public AStarNode(int x, int y, int h, float g, float hCost, AStarNode parent)
        {
            this.x = x;
            this.y = y;
            this.h = h;
            this.g = g;
            this.hCost = hCost;
            this.parent = parent;
        }
    }

    private static readonly Vector2Int[] OFFSETS =
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

    private static float Heuristic(int x, int y, int h, int tx, int ty, int th)
    {
        float dx = x - tx;
        float dy = y - ty;
        float dh = h - th;
        return Mathf.Sqrt(dx * dx + dy * dy + dh * dh);
    }


    // -----------------------------------------------------
    // Complete A* pathfinding
    // -----------------------------------------------------
    public static List<Vector3> FindPath(Vector3 worldStart, Vector3 worldEnd)
    {
        if (!NavGridGen.WorldToGrid(worldStart, out int sx, out int sy, out int sh))
        {
            Debug.LogWarning("Start outside grid");
            return null;
        }

        if (!NavGridGen.WorldToGrid(worldEnd, out int tx, out int ty, out int th))
        {
            Debug.LogWarning("Target outside grid");
            return null;
        }

        NodeGrid startGrid = NavGridGen.grid[sx, sy];
        NodeGrid targetGrid = NavGridGen.grid[tx, ty];

        if (startGrid.heights == null || startGrid.heights.Length == 0 ||
            targetGrid.heights == null || targetGrid.heights.Length == 0)
            return null;

        List<AStarNode> openList = new List<AStarNode>();
        HashSet<(int, int, int)> closed = new HashSet<(int, int, int)>();

        openList.Add(new AStarNode(sx, sy, sh, 0f, Heuristic(sx, sy, sh, tx, ty, th), null));

        // -----------------------------------------------------
        // A* MAIN LOOP
        // -----------------------------------------------------
        while (openList.Count > 0)
        {
            // Pick best f
            AStarNode current = openList[0];
            for (int i = 1; i < openList.Count; i++)
                if (openList[i].f < current.f)
                    current = openList[i];

            // End reached
            if (current.x == tx && current.y == ty && current.h == th)
                return ReconstructPath(current);

            openList.Remove(current);
            closed.Add((current.x, current.y, current.h));

            NodeGrid node = NavGridGen.grid[current.x, current.y];

            // -----------------------------------------------------
            // Explore neighbors
            // -----------------------------------------------------
            for (int d = 0; d < 8; d++)
            {
                int nx = current.x + OFFSETS[d].x;
                int ny = current.y + OFFSETS[d].y;

                if (nx < 0 || nx >= NavGridGen.gridX || ny < 0 || ny >= NavGridGen.gridY)
                    continue;

                NodeGrid neigh = NavGridGen.grid[nx, ny];
                if (neigh.heights == null || neigh.heights.Length == 0)
                    continue;

                // Respect connections for this height
                if (node.connections == null || !node.connections[current.h][d])
                    continue;

                float currentHeight = node.heights[current.h];

                // -----------------------------------------------------
                // CRUCIAL FIX:
                // Test ALL heights of the neighbor, ignore insideGeometry
                // -----------------------------------------------------
                // Trouver la hauteur la plus proche valide (non bloquée) chez le voisin
                int nh = FindNearestValidHeight(neigh, currentHeight);

                if (nh < 0)
                    continue; // pas de hauteur valide proche, on ignore ce voisin

                if (closed.Contains((nx, ny, nh)))
                    continue;

                Vector3 posA = NavGridGen.GridToWorld(current.x, current.y);
                posA.y = currentHeight;

                Vector3 posB = NavGridGen.GridToWorld(nx, ny);
                posB.y = neigh.heights[nh];

                float cost = Vector3.Distance(posA, posB);

                float newG = current.g + cost;
                float newHCost = Heuristic(nx, ny, nh, tx, ty, th);

                AStarNode exists = openList.Find(n => n.x == nx && n.y == ny && n.h == nh);

                if (exists == null)
                {
                    openList.Add(new AStarNode(nx, ny, nh, newG, newHCost, current));
                }
                else if (newG < exists.g)
                {
                    exists.g = newG;
                    exists.parent = current;
                }

            }
        }

        return null;
    }

    private static int FindNearestValidHeight(NodeGrid node, float targetHeight)
    {
        if (node.heights == null || node.isInsideGeometry == null)
            return -1;

        float bestDiff = float.MaxValue;
        int bestIndex = -1;

        for (int i = 0; i < node.heights.Length; i++)
        {
            if (i < node.isInsideGeometry.Length && node.isInsideGeometry[i])
                continue; // bloqué

            float diff = Mathf.Abs(node.heights[i] - targetHeight);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestIndex = i;
            }
        }

        return bestIndex;
    }



    // -----------------------------------------------------
    // Build final path
    // -----------------------------------------------------
    private static List<Vector3> ReconstructPath(AStarNode end)
    {
        List<Vector3> path = new List<Vector3>();
        AStarNode cur = end;

        while (cur != null)
        {
            Vector3 pos = NavGridGen.GridToWorld(cur.x, cur.y);
            pos.y = NavGridGen.grid[cur.x, cur.y].heights[cur.h];
            path.Add(pos);

            cur = cur.parent;
        }

        path.Reverse();
        return path;
    }
}
