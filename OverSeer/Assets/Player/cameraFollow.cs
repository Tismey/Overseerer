using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class cameraFollow : MonoBehaviour
{
    HashSet<AIbase> players = new HashSet<AIbase>();

    public Transform target;

    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        var l = AIbase.Population;
        foreach (AIbase ai in l) { 
        
            if (ai.isPlayer)
            {
                players.Add(ai);
            }
        }

        Vector3 sum = Vector3.zero;
        foreach (PlayerState player in players)
        {
            sum += player.transform.position;
        }

        Vector3 averagePosition = sum / players.Count;
        Debug.Log("count =" + players.Count);
        if (players.Count == 0)
        {
           ;
        }
        else
        {
            transform.position = averagePosition + offset;
        }
        
    }
}
