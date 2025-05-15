using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public TextMeshProUGUI text;
    public Slider slider;
    public Image sliderImage;
    public AIbase aiBase;
    public Healthcontroller healthController;
    public SetCrosshair setCrosshair;

    [Header("Colors")]
    [Tooltip("Couleur commune à tous les éléments UI (barre de vie, texte munitions, etc.)")]
    [SerializeField] private Color uiColor = Color.green;
    // Start is called before the first frame update
    void Start()
    {
        sliderImage.color = uiColor;
        text.color = uiColor;
        setCrosshair.SetCrosshairColor(uiColor);
    }

    // Update is called once per frame
    void Update()
    {
        if(aiBase.weapon != null)
        {
            text.text = aiBase.weapon.currentClip.ToString() + "/" + aiBase.weapon.currentAmmo.ToString();
        }
        else
        {
            text.text = "";
        }

        if (healthController != null)
        {
            slider.value = healthController.GetHealth();
        }
        else
        {
            slider.value = 0;
        }
    }
}
