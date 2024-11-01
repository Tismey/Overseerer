using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawntest : MonoBehaviour
{

    public Transform player;
    public GameObject spawnEntitie;
    public int numOfSpawn;
    private int numOfSpawnStart;
    public float spawnRate;
    public int Wavenumber;
    public int WaveDelay;
    private float nextWave = 0;
    private float nextSpawn = 0;
    // Start is called before the first frame update
    void Start()
    {
        numOfSpawnStart = numOfSpawn; 
    }

    // Update is called once per frame
    void Update()
    {
        if(AIbase.Population.Count > 30)
        {
            return;
        }
        nextSpawn += Time.deltaTime;
        if(nextSpawn >= spawnRate && numOfSpawn > 0 && Wavenumber > 0 )
        {
              nextSpawn = 0;
              numOfSpawn--;
              GameObject go = Instantiate(spawnEntitie,transform.position, Quaternion.identity);
              go.GetComponent<AIbase>().AddState(new ChaseTest(player));
              

        }
        if(numOfSpawn == 0 && Wavenumber > 0)
        {
            nextWave += Time.deltaTime;
            if(nextWave >= WaveDelay)
            {
                Wavenumber--;
                numOfSpawn = numOfSpawnStart;
                nextWave = 0;
                WaveDelay -= 1;
            }
            
        }
    }
}
