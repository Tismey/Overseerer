using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetCrosshair : MonoBehaviour
{
    public RawImage[] images = new RawImage[4];
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetCrosshairColor(Color color)
    {
        foreach (RawImage image in images)
        {
            image.color = color;
        }
    }   

    // Update is called once per frame
    void Update()
    {
        
    }
}
