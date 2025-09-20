using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public TextMeshProUGUI text;
    public TextMeshProUGUI timer;
    public TextMeshProUGUI textInteract;
    public Slider slider;
    public Image sliderImage;
    public AIbase aiBase;
    public Healthcontroller healthController;
    public SetCrosshair setCrosshair;
    public InteractionManager interactionManager;

    private float time = 0f;    

    [Header("Colors")]
    [Tooltip("Couleur commune à tous les éléments UI (barre de vie, texte munitions, etc.)")]
    [SerializeField] private Color uiColor = Color.green;
    // Start is called before the first frame update
    void Start()
    {
        sliderImage.color = uiColor;
        text.color = uiColor;
        timer.color = uiColor;
        textInteract.color = uiColor;
        setCrosshair.SetCrosshairColor(uiColor);
        interactionManager = GetComponent<InteractionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        int sec = (int)time % 60;
        int min = (int)time / 60;
        if (aiBase.weapon[aiBase.WeaponSelect] != null)
        {
            text.text = aiBase.weapon[aiBase.WeaponSelect].currentClip.ToString() + "/" + aiBase.weapon[aiBase.WeaponSelect].currentAmmo.ToString();
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

        if (interactionManager != null)
        {
            Debug.Log("InteractionManager not null");
            if (interactionManager.CanInteractWith(out Iteractebable inter))
            {
                textInteract.text = inter.getInteractLabel();
            }
            else
            {
                textInteract.text = "";
            }
        }
        else
        {
            textInteract.text = "";
        }

        timer.text = min.ToString("00") +":" + sec.ToString("00") ;
    }
}
