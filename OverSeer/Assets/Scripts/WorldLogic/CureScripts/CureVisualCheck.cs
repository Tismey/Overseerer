using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CureVisualCheck : MonoBehaviour
{
    public MeshRenderer[] meshes = new MeshRenderer[5];
    public Material[] mat = new Material[3];
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void UpdateVisual(CureCrafterManager.correlation[] cor)
    {
        for(int i = 0; i < 5; i++)
        {
            if(cor[i] == CureCrafterManager.correlation.INR)
            {
                meshes[i].material = mat[0];
            }
            else if(cor[i] == CureCrafterManager.correlation.INW)
            {
                meshes[i].material = mat[1];
            }
            else
            {
                meshes[i].material = mat[2];
            }
        }
    }
}
