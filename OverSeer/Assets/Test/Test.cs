using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{   
    public Transform target;
    public AIbase ai;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //shoot a ray out of the camera when it hit a valid navmesh point add state move to the ai with the point as the target
        if(Input.GetMouseButtonDown(0))
            RaycastAndMoveToTarget();

        if(Input.GetKeyDown(KeyCode.Space))
        {
            ai.AddState(new Stumble());
        }
    }

    private void RaycastAndMoveToTarget()
    {
        // Shoot a ray out of the camera
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the ray hits a valid navmesh point
        if (Physics.Raycast(ray, out hit))
        {
            target.position = hit.point;
            // Add state move to the ai with the point as the target
            ai.AddState(new Moving(hit.point));
        }
    }
}
