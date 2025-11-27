using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarTest : MonoBehaviour
{

    public Transform start;
    public Transform end;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        List<Vector3> path = AStarPathFinder.FindPath(start.position, end.position);

        if (path != null)
        {
            foreach (var p in path)
                Debug.DrawLine(p, p + Vector3.up * 2, Color.magenta, 5f);
        }
    }
}
