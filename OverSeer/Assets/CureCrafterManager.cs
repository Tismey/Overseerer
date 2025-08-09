using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CureCrafterManager : MonoBehaviour
{

    public enum correlation
    {
        NOTIN,
        INW,
        INR,

    }

    public List<correlation[]> correlations = new List<correlation[]>();

    private const int MAXNODES = 5;

    private bool createCure = false;

    public CureNodeInteract[] cureCombination = new CureNodeInteract[MAXNODES];

    private CureNodeInteract.NodeType[] secretCOmbination;

    private int[] occurences;
    // Start is called before the first frame update

    public bool CreateCure()
    {
        foreach (CureNodeInteract cur in cureCombination)
        {
            if (cur.getCurtype() == CureNodeInteract.NodeType.NONE)
            {
                createCure = false;
                return false;
            }
        }
        createCure = true;
        return true;
    }
    void Start()
    {

    
        secretCOmbination = new CureNodeInteract.NodeType[MAXNODES];
        occurences = new int[4];
        //generate the secret combination
        for (int i = 0; i < MAXNODES; i++)
        {
            secretCOmbination[i] = (CureNodeInteract.NodeType)Random.Range(1, 4);
            occurences[(int)secretCOmbination[i] - 1]++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (createCure)
        {
            var cr = new correlation[5];
            var tmp = new int[4];
            occurences.CopyTo(tmp, 0);
            foreach(int i in tmp)
            {
                Debug.Log((CureNodeInteract.NodeType)(i+1) + " occurs " + tmp[i]);
            }
           
            for (int i = 0; i < MAXNODES; i++)
            {
                if(cureCombination[i].getCurtype() == secretCOmbination[i])
                {
                    cr[i] = correlation.INR;
                    tmp[(int)cureCombination[i].getCurtype() - 1]--;
                }
                else if(tmp[(int)cureCombination[i].getCurtype() -1] > 0)
                {
                    cr[i] = correlation.INW;
                    tmp[(int)cureCombination[i].getCurtype() - 1]--;
                }
                else
                {
                    cr[i] = correlation.NOTIN;
                }
            }

            correlations.Add(cr);
        }
        createCure = false;
    }
}
