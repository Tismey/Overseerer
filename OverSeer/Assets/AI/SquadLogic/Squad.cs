using System.Collections.Generic;
using UnityEngine;

public class Squad : MonoBehaviour
{
    public List<AIbase> soldiers = new List<AIbase>();
    public AIbase leader;

    public Vector3 destination;
    public Vector3 coverFrom;

    public List<Vector3> path;
    public int currentPathIndex = 0;

    private SquadState currentState;

    // Tous les states injectés par le squad
    public List<AIState> squadInjectedStates = new List<AIState>();

    void Update()
    {
        // Nettoyage des morts
        soldiers.RemoveAll(s => s == null);

        // Relead si besoin
        if (leader == null && soldiers.Count > 0)
        {
            leader = soldiers[0];
            ReassignFormation();
        }

        currentState?.Update();
    }

    public void SetState(SquadState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    // -------- State ownership --------

    public void PushSquadState(AIbase ai, AIState state)
    {
        if (ai == null || state == null)
            return;

        ai.AddState(state);
        squadInjectedStates.Add(state);
    }

    public void FinishSquadStates()
    {
        foreach (var s in squadInjectedStates)
        {
            if (s != null && !s.Ended())
                s.Finish();
        }
        squadInjectedStates.Clear();
    }

    // -------- Formation --------

    public void ReassignFormation()
    {
        if (leader == null)
            return;

        FinishSquadStates();

        // Le leader reprend le mouvement courant
        if (path != null && currentPathIndex < path.Count)
        {
            PushSquadState(leader, new MovingAndClimb(path[currentPathIndex]));
        }

        // Les autres suivent
        for (int i = 1; i < soldiers.Count; i++)
        {
            PushSquadState(soldiers[i], new Follow(this));
        }
    }

    public List<Vector3> FindCoverPoints(
    Vector3 origin,
    Vector3 threatDir,
    float radius,
    int maxPoints
)
    {
        List<Vector3> results = new List<Vector3>();
        List<Vector3> candidates = new List<Vector3>();

        int dirIndex = NavGridGen.GetClosestDirectionIndex(threatDir);
        if (dirIndex < 0)
            return results;

        // Rayon en monde → approx rayon en cellules
        float cellSizeX = Mathf.Abs(
            NavGridGen.GridToWorld(1, 0).x - NavGridGen.GridToWorld(0, 0).x
        );
        int cellRadius = Mathf.CeilToInt(radius / cellSizeX);

        // Centre grille
        if (!NavGridGen.WorldToGrid(origin, out int ci, out int cj, out _))
            return results;

        // 1️⃣ Collecte brute
        for (int i = ci - cellRadius; i <= ci + cellRadius; i++)
        {
            for (int j = cj - cellRadius; j <= cj + cellRadius; j++)
            {
                if (i < 0 || i >= NavGridGen.gridX ||
                    j < 0 || j >= NavGridGen.gridY)
                    continue;

                NodeGrid node = NavGridGen.grid[i, j];
                if (node.heights == null)
                    continue;

                Vector3 basePos = NavGridGen.GridToWorld(i, j);

                for (int h = 0; h < node.heights.Length; h++)
                {
                    if (node.isInsideGeometry[h])
                        continue;

                    if (!node.cover[h][dirIndex])
                        continue;

                    Vector3 pos = basePos;
                    pos.y = node.heights[h];

                    if (Vector3.Distance(origin, pos) > radius)
                        continue;

                    candidates.Add(pos);
                }
            }
        }

        if (candidates.Count == 0)
            return results;

        // 2️⃣ Tri par distance à l'origine
        candidates.Sort((a, b) =>
            Vector3.Distance(origin, a)
            .CompareTo(Vector3.Distance(origin, b))
        );

        // 3️⃣ Sélection homogène
        float minSeparation = radius / Mathf.Max(1, maxPoints);

        foreach (Vector3 c in candidates)
        {
            bool tooClose = false;

            foreach (Vector3 r in results)
            {
                if (Vector3.Distance(c, r) < minSeparation)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            results.Add(c);

            if (results.Count >= maxPoints)
                break;
        }

        return results;
    }

}
