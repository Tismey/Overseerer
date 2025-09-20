using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WordLogic : MonoBehaviour
{

    public GameObject m_player;
    public GameObject m_ZombieSpawnManager;
    public GameObject m_cureCrafter;
    public GameObject m_cureChecker;

    public PlayerState s_player;
    public zombiSpawnMananger zsm;
    public CureCrafterManager ccm;
    public CureVisualCheck cvc;
    public BedLogic bed;



    public SampleInteract redSample;
    public SampleInteract blueSample;
    public SampleInteract greenSample;

    public bool gameStart = true;
    public bool objectiveComplete = false;
    public bool resetWord = false;


    // Start is called before the first frame update
    void Start()
    {
        s_player = m_player.GetComponent<PlayerState>();
        zsm = m_ZombieSpawnManager.GetComponent<zombiSpawnMananger>();
        ccm = m_cureCrafter.GetComponent<CureCrafterManager>();
        cvc = m_cureChecker.GetComponent<CureVisualCheck>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStart)
        {
            zsm.StartSpawns();
            if (objectiveComplete)
            {
                bed.setInteract(true);
            }
        }
    }

    public void Reset()
    {
        DestroyZombies();
        zsm.StartSpawns();
        ccm.Reset();
        redSample.setInteract(true);
        blueSample.setInteract(true);
        greenSample.setInteract(true);
        objectiveComplete = false;
        for(int  i = 0; i < 3; i++)
        {
            s_player.NodesInPossesion[i] = false;
        }

    }

    public void DestroyZombies()
    {
        var l = AIbase.Population;
        foreach(AIbase a in l)
        {
            if(a.gameObject.layer == 9)
            {
                Destroy(a.gameObject);
            }
        }
    }
}
