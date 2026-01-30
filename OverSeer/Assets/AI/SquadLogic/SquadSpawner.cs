using System.Collections.Generic;
using UnityEngine;

public class SquadSpawner : MonoBehaviour
{
    [Header("Soldiers")]
    public GameObject soldierPrefab;
    public int soldierCount = 4;
    public float spawnRadius = 2.5f;

    [Header("Weapons")]
    public GameObject[] weaponPrefabs;

    [Header("Orders")]
    public Transform initialDestination;
    public Transform coverFrom;

    private Vector3 danger;

    public string team;
    List<AIbase> soldiers;

    private bool init = false;


    // --------------------------------------------------
    void Awake()
    {
        SpawnSquad();
    }

    // --------------------------------------------------
    void SpawnSquad()
    {
       soldiers = new List<AIbase>();

        for (int i = 0; i < soldierCount; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();

            GameObject go = Instantiate(
                soldierPrefab,
                spawnPos,
                Quaternion.identity
            );

            AIbase ai = go.GetComponent<AIbase>();
            if (ai == null)
            {
                Debug.LogError("Soldier prefab has no AIbase");
                Destroy(go);
                continue;
            }

            soldiers.Add(ai);
            ai.teamName = team;
        }

        danger = coverFrom.position - soldiers[0].transform.position;
        if (soldiers.Count == 0)
            return;

        // Leader = premier vivant
        
        
       
    }

    private void Update()
    {
        if (!init)
        {
            if (NavGridGen.ready)
            {
                InitSquad();
                GiveWeapons(soldiers);
                init = true;
            }
        }
    }

    // --------------------------------------------------
    Vector3 GetRandomSpawnPosition()
    {
        Vector2 rnd = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = transform.position + new Vector3(rnd.x, 0f, rnd.y);

        // Projection au sol
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 20f, Vector3.down, out hit, 50f))
        {
            pos.y = hit.point.y;
        }

        return pos;
    }

    // --------------------------------------------------
    void GiveWeapons(List<AIbase> soldiers)
    {
        foreach (var ai in soldiers)
        {
            if (weaponPrefabs.Length == 0)
                continue;

            WeaponAbstract w = Instantiate(
                weaponPrefabs[Random.Range(0, weaponPrefabs.Length)].gameObject,
                Vector3.zero, Quaternion.identity
            ).GetComponent<WeaponAbstract>() ;

            w.WeaponPickUp(ai);
        }
    }

    void InitSquad()
    {

        foreach(AIbase a in soldiers)
        {
            a.AddState(new TacticalMove(initialDestination.position, danger, false, false));
        }

    }
    
}
