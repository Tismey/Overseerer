using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ORDERTYPE
{
    MOVEANDHOLD,
    DEFEND,
    MOVEANDTRAP,
    SEEKANDDESTROY

}

public class SquadLeader : AIState
{
    // Start is called before the first frame update
    private float staggerTime = 0f;
    private ORDERTYPE orderType;
    private Vector3 pos;
    private Vector3 danger;
    private List<AIbase> squad;


    private List<Vector3> path;
   

    public SquadLeader(Vector3 pos,ORDERTYPE o, Vector3 danger,List<AIbase> soldiers)
    {
        orderType = o;
        this.pos = pos;
        this.danger = danger;
        squad = soldiers;

    }

    public override void Setup()
    {

     


        if (orderType == ORDERTYPE.MOVEANDHOLD)
        {
            ai.AddState(new TacticalMove(pos, danger, false,false));
            for(int i = 1; i < squad.Count; i++)
            {
                squad[i].AddState(new Follow(squad,this));
            }
        }

    }

    public override void act()
    {

        if(orderType == ORDERTYPE.MOVEANDHOLD)
        {
            moveandhold();
        }
     

    }

    public override void Interupt()
    {
      
    }

    public override void Finish()
    {
      
    }

    public override void Continue()
    {

    }

    private void moveandhold()
    {

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
        List<Vector3> fallbackCandidates = new List<Vector3>();

        int dirIndex = NavGridGen.GetClosestDirectionIndex(threatDir);
        if (dirIndex < 0)
            return results;

 
        int cellRadius = (int)radius;

        if (!NavGridGen.WorldToGrid(origin, out int ci, out int cj, out _))
            return results;

        // 1️⃣ Collecte
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

                    Vector3 pos = basePos;
                    pos.y = node.heights[h];


                    // ✔ vrai couvert
                    if (node.cover[h][dirIndex])
                    {
                        candidates.Add(pos);
                    }
                    // ❌ pas couvert → fallback possible
                    else
                    {
                        fallbackCandidates.Add(pos);
                    }
                }
            }
        }

        // 2️⃣ Tri par distance
        candidates.Sort((a, b) =>
            Vector3.Distance(origin, a)
            .CompareTo(Vector3.Distance(origin, b))
        );

        float minSeparation = radius / Mathf.Max(1, maxPoints);

        // 3️⃣ Sélection des vrais couverts
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
                return results;
        }

        // 🔁 FALLBACK : compléter avec des points walkables aléatoires
        if (results.Count < maxPoints && fallbackCandidates.Count > 0)
        {
            // Mélange aléatoire
            for (int i = 0; i < fallbackCandidates.Count; i++)
            {
                int rnd = Random.Range(i, fallbackCandidates.Count);
                (fallbackCandidates[i], fallbackCandidates[rnd]) =
                    (fallbackCandidates[rnd], fallbackCandidates[i]);
            }

            foreach (Vector3 c in fallbackCandidates)
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
        }

        return results;
    }
}

