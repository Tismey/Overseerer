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

    public CureVisualCheck cvc;

    public WordLogic wl;

    public List<correlation[]> correlations = new List<correlation[]>();

    private const int MAXNODES = 5;

    private bool createCure = false;

    public CureNodeInteract[] cureCombination = new CureNodeInteract[MAXNODES];

    private CureNodeInteract.NodeType[] secretCOmbination;

    public CureActivateInteract cureButton;

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
        occurences = new int[3];
        //generate the secret combination
        for (int i = 0; i < MAXNODES; i++)
        {
            secretCOmbination[i] = (CureNodeInteract.NodeType)Random.Range(1, 3);
            Debug.Log(i + 1 + "in secret is " + secretCOmbination[i]);
            occurences[(int)secretCOmbination[i] - 1]++;
        }
    }

    public void Reset()
    {
        foreach(CureNodeInteract cc in cureCombination)
        {
            cc.Reset();
            cureButton.Reset();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (createCure)
        {
            var cr = new correlation[5];
            var tmp = new int[3];
            occurences.CopyTo(tmp, 0);
           //This Nees to be fixed (double loop?)
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


            cvc.UpdateVisual(cr);
            wl.objectiveComplete = true;
        }
        createCure = false;
    }
}
