using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class pupilScript : MonoBehaviour
{





    [Header("Reférences")]
    [Tooltip("L'os de la tête (parent direct de la pupille)")]
    public Transform headBone;
    [Tooltip("La cible à regarder (Camera.main par défaut si non assigné)")]
    public Transform target;

    [Header("Réglages")]
    [Tooltip("Rayon maximum (en unités locales) du déplacement de la pupille")]
    public float maxRadius = 0.1f;

    Vector3 baseLocalPos;

    void Awake()
    {
        baseLocalPos = transform.localPosition;
        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        var player = GameObject.Find("PlayefpsController");
        var ai = player.GetComponent<AIbase>();
        if (ai != null)
        {
            target = ai.eyePosition;
        }
    }

    void LateUpdate()
    {
        if (headBone == null || target == null)
            return;

        // 1) direction monde de la tête vers la cible
        Vector3 toTarget = (target.position - headBone.position).normalized;

        // 2) ramène dans le repère local de la tête
        Vector3 localDir = headBone.InverseTransformDirection(toTarget);

        // 3) calcule les angles yaw (autour de Y) et pitch (autour de X)
        float yaw = Mathf.Atan2(localDir.x, localDir.z);  // gauche/droite
        float pitch = Mathf.Atan2(localDir.y, localDir.z);  // haut/bas

        // 4) deux tangentes pour obtenir un déplacement proportionnel à l’angle
        float xOffset = -Mathf.Tan(yaw) * maxRadius;
        float yOffset = Mathf.Tan(pitch) * maxRadius;

        // 5) clamp dans un cercle de rayon maxRadius
        Vector2 clamped = Vector2.ClampMagnitude(new Vector2(xOffset, yOffset), maxRadius);

        // 6) on applique autour de la position de base
        transform.localPosition = baseLocalPos + new Vector3(clamped.x, clamped.y, 0f);
        
        



    }



}

