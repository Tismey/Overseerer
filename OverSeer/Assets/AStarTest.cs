using System.Collections.Generic;
using UnityEngine;

public class AStarTest : MonoBehaviour
{
    public Transform start;
    public Transform end;
    public Transform coverFrom;

    public Transform player;   // NEW: for shoulder aiming test
    public GameObject AiTest;  // Prefab of your AI



    void FixedUpdate()
    {
        if (start == null || end == null)
            return;

        Vector3 dir = coverFrom != null
            ? coverFrom.position - start.position
            : end.position - start.position;

        //List<Vector3> path = AStarPathFinder.FindPath(start.position, end.position);

        //List<Vector3> pathCover = AStarPathFinder.FindPathWithCover(
        //  start.position,
        //end.position,
        //GetClosestDirectionIndex(dir)
        //);

        /*if (path != null)
        {
            foreach (var p in path)
                Debug.DrawLine(p, p + Vector3.up * 2, Color.magenta, 0.1f);
        }

        if (pathCover != null)
        {
            foreach (var p in pathCover)
                Debug.DrawLine(p, p + Vector3.up * 2, Color.cyan, 0.1f);
        }
    */}



    // --------------------------------------------------------------------
    // TEST 1: Spawn AI and Move (existing)
    // --------------------------------------------------------------------
    public void SpawnAIAndMove()
    {
        if (!CheckBasic())
            return;

        GameObject go = Instantiate(AiTest, start.position, Quaternion.identity);
        var ai = go.GetComponent<AIbase>();

        ai.AddState(new MovingAndClimb(end.position));

        Debug.Log("AI spawned and path state assigned!");
    }


    // --------------------------------------------------------------------
    // TEST 2: IA with rotating shoulder (incrementing angle)
    // --------------------------------------------------------------------
    public void SpawnAISimpleShoulderRotation()
    {
        if (!CheckBasic())
            return;

        GameObject go = Instantiate(AiTest, start.position, Quaternion.identity);
        var ai = go.GetComponent<AIbase>();

        ai.AddState(new TestShoulderRotateState());

        Debug.Log("AI spawned with simple shoulder rotation state!");
    }


    // --------------------------------------------------------------------
    // TEST 3: IA looking toward the target, aiming shoulder at player
    // --------------------------------------------------------------------
    public void SpawnAIAimAtPlayer()
    {
        if (!CheckBasic())
            return;

        if (player == null)
        {
            Debug.LogError("Player reference is missing!");
            return;
        }

        GameObject go = Instantiate(AiTest, start.position, Quaternion.identity);
        var ai = go.GetComponent<AIbase>();

        ai.AddState(new TestShoulderAimAtPlayerState(player));

        Debug.Log("AI spawned: will look and aim shoulder at player!");
    }



    // --------------------------------------------------------------------
    // Utility
    // --------------------------------------------------------------------
    private bool CheckBasic()
    {
        if (AiTest == null)
        {
            Debug.LogError("AiTest prefab missing!");
            return false;
        }

        if (start == null)
        {
            Debug.LogError("Start transform missing!");
            return false;
        }

        return true;
    }



    // --------------------------------------------------------------------
    // Direction index logic (unchanged)
    // --------------------------------------------------------------------
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
}
