using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zombiSpawnMananger : MonoBehaviour
{
    public bool spawnZombies;

    private bool spawnHordes = true;
    
    public GameObject zombiePrefab;

    public GameObject hordePrefab;

    public Transform playerTransform;

    public LayerMask groundMask;

    public LayerMask playerMask;

    public float hordeTimer = 0f;

    public float spawnRadius;

    public float spawnDistance;

    public float spawnHeightMax;

    public int SpawnChance;

    public int playerHeat = 0;//not setup yet

    public int hordeSpawnChance;

    public const int MAXSPAWN = 20;

    private bool startSpawning = false;

    private float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartSpawns()
    {
        startSpawning = true;
    }

    public void StopSpawns()
    {
        startSpawning = false;
    }
    // Update is called once per frame
    void Update()
    {

        timer += Time.deltaTime;
        hordeTimer += Time.deltaTime;

        if (!spawnZombies) return;
        if (startSpawning && timer > 0.1f)
        {
            timer = 0;
            if (AIbase.Population.Count > MAXSPAWN ) return;
            var spawnVec = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-10, spawnHeightMax), Random.Range(-spawnRadius, spawnRadius));
            spawnVec += playerTransform.position + playerTransform.forward *  spawnDistance;

            RaycastHit hit;
            if (Physics.Raycast(spawnVec, Vector3.down, out hit, 200f, groundMask))
            {
                var pos = hit.point;
                if (Physics.Linecast(hit.point + (Vector3.up * 5),playerTransform.position, groundMask))
                {
                    if(Random.Range(0,20) < SpawnChance)
                    {
                        var zomb = Instantiate(zombiePrefab, pos, Quaternion.identity);
                        zomb.GetComponent<AIbase>().AddState(new Idle());
                    }

                    if(hordeTimer > 30f)
                    {
                        hordeTimer = 0f;
                        if (Random.Range(0, 200) < hordeSpawnChance - AIbase.Population.Count)
                        {
                            Instantiate(hordePrefab, pos, Quaternion.identity);
                        }
                    }
                        
                }
            }
        }


    }
}
