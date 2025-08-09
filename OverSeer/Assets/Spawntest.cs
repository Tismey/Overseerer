using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    HashSet<AIbase> players = new HashSet<AIbase>();
    // Start is called before the first frame update
    void Start()
    {
        numOfSpawnStart = numOfSpawn; 
    }

    // Update is called once per frame
    void Update()
    {

        var l = AIbase.Population;
        foreach (AIbase ai in l)
        {

            if (ai.isPlayer)
            {
                players.Add(ai);
            }
        }
        if(players.Count == 0)
        {
            Debug.Log("No players");
            return;
        }
        if (AIbase.Population.Count > 1000)
        {
            return;
        }
        
        AIbase randomPlayer = players.ElementAt(Random.Range(0, players.Count));
        nextSpawn += Time.deltaTime;
        if(nextSpawn >= spawnRate && numOfSpawn > 0 && Wavenumber > 0 )
        {
            Debug.Log("Spawning");
            nextSpawn = 0;
              numOfSpawn--;
              GameObject go = Instantiate(spawnEntitie,transform.position, Quaternion.identity);
              go.GetComponent<AIbase>().AddState(new ChaseTest(randomPlayer.transform));
              

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
