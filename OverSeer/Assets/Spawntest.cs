using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawntest : MonoBehaviour
{

    public Transform player;
    public GameObject spawnEntitie;
    public int numOfSpawn;
    public float spawnRate;

    private float nextSpawn = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        nextSpawn += Time.deltaTime;
        if(nextSpawn >= spawnRate && numOfSpawn > 0  )
        {
              nextSpawn = 0;
              numOfSpawn--;
              GameObject go = Instantiate(spawnEntitie,transform.position, Quaternion.identity);
              go.GetComponent<AIbase>().AddState(new ChaseTest(player));
            
        }
    }
}
