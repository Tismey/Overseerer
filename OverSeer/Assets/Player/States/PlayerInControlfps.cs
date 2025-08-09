using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInControlfps : AIState
{
    public Camera cam;
    public int numberOfRays = 10; // Nombre de raycasts
    public float arcAngle = 90f;  // Angle de l'arc en degrés
    public float rayDistance = 5f; // Distance des raycasts
    public float shoveCooldown = 1f;
    private float shoveTimer = 0f;
    private PlayerMouvement playerMouvement;
    private InteractionManager interactionManager;
    private Iteractebable interactebale;

    [Header("Recoil Settings")]
    [Tooltip("Angle (in degrees) the camera is kicked up on fire.")]
    public float recoilAngle = 20f;
    [Tooltip("How snappy the kick is (higher → faster).")]
    public float snappiness = 2f;
    [Tooltip("How quickly the camera returns to neutral.")]
    public float returnSpeed = 30f;
    private Quaternion baseRotation;
    public Transform recoilJoint;
    // Internal state for recoil
    private float currentRecoil = 0f;
    private float targetRecoil = 0f;

    [Header("Shake Settings")]
    [Tooltip("Amplitude max du shake (en degrés)")]
    public float shakeIntensity = 1f;
    [Tooltip("Vitesse de décroissance du shake")]
    public float shakeDecay = 5f;

    float shakeAmount;
    Vector2 jitter;

    public override void Setup()
    {
        this.Animator.SetBool("Moving", true);
        this.ai.lockRoot.Lock();
        this.ai.canMove = true;
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        playerMouvement = ai.GetComponent<PlayerMouvement>();
        interactionManager = ai.GetComponent<InteractionManager>();
        recoilJoint = ai.eyePosition.parent.transform;
        baseRotation = recoilJoint.localRotation;


    }
    public override void act()
    {
        //do nothing
        //this.Animator.SetBool("Idle",true);
        shoveTimer += Time.deltaTime;
        UpdateRecoil();
        if (playerMouvement.ShoveInput && shoveTimer > shoveCooldown)
        {
            CastArcRays();
            
            //this.Animator.SetTrigger("Shove");
        }
        if (playerMouvement.ShootInput && ai.weapon[ai.WeaponSelect] != null)
        {
            ai.weapon[ai.WeaponSelect].Shoot(ai.eyePosition.forward,ai.eyePosition.position);
            if(ai.weapon[ai.WeaponSelect].CanShoot())
                ApplyRecoil();
        }

        if (playerMouvement.dropInput && ai.weapon[ai.WeaponSelect] != null)
        {
            Debug.Log("drop The gun");
            ai.weapon[ai.WeaponSelect].PutDown();
        }

        if(playerMouvement.switchInput != 0)
        {
            ai.WeaponSelect += playerMouvement.switchInput;
            if(ai.WeaponSelect < 0)
            {
                ai.WeaponSelect = 8;
            }
            if(ai.WeaponSelect <= 9)
            {
                ai.WeaponSelect = 0;
            }
            Debug.Log("Weapon : " + ai.WeaponSelect + " selected");
        }

        

        if (interactionManager.CanInteractWith(out interactebale))
        {
            if(playerMouvement.interactInput)
            {
                Debug.Log("Interat input");
                interactebale.Interact(ai);
                playerMouvement.interactInput = false;
            }
        }

    }

    public override void Interupt()
    {
        this.ai.canMove = false;
        this.Animator.SetBool("Moving", false);
        this.ai.lockRoot.Lock();
    }

    public override void Finish()
    {
        this.ai.canMove = false;
        this.Animator.SetBool("Moving", false);
    }

    public override void Continue()
    {
        this.ai.canMove = true;
        this.ai.lockRoot.MoveWithAnim();
        this.Animator.SetBool("Moving", true);
    }

    void CastArcRays()
    {
        // Calcul de l'angle de départ et d'incrément
        float startAngle = -arcAngle / 2;
        float angleIncrement = arcAngle / (numberOfRays - 1);

        for (int i = 0; i < numberOfRays; i++)
        {
            // Calcul de l'angle en radians
            float angle = startAngle + i * angleIncrement;
            float angleRad = Mathf.Deg2Rad * angle;

            // Direction du raycast en fonction de l'angle
            Vector3 direction = new Vector3(Mathf.Sin(angleRad), 0, Mathf.Cos(angleRad));
            direction = ai.transform.TransformDirection(direction); // Adapter à l'orientation du personnage

            // Lancer le raycast
            if (Physics.Raycast(ai.transform.position, direction, out RaycastHit hit, rayDistance))
            {
                Debug.DrawLine(ai.transform.position, hit.point, Color.red); // Visualiser les raycasts touchés
                if(hit.collider.gameObject.GetComponent<AIbase>() != null)
                {
                    hit.collider.gameObject.GetComponent<AIbase>().AddState(new Stumble());
                    shoveTimer = 0f;
              
                }
            }
            else
            {
                Debug.DrawRay(ai.transform.position, direction * rayDistance, Color.green); // Visualiser les raycasts non touchés
            }
        }
    }

    private void UpdateRecoil()
    {
        // 1) Recoil interp
        // 1) Recoil interpolation
        currentRecoil = Mathf.Lerp(currentRecoil, targetRecoil, Time.deltaTime * snappiness);
        targetRecoil = Mathf.Lerp(targetRecoil, 0f, Time.deltaTime * returnSpeed);

        // 2) Shake decay
        shakeAmount = Mathf.Lerp(shakeAmount, 0f, Time.deltaTime * shakeDecay);

        // 3) Random jitter vector

        // 4) Build a combined rotation:
        //    – recoil on pitch (X)
        //    – shake on pitch (Y jitter) and yaw (X jitter)
        Quaternion recoilRot = Quaternion.Euler(-currentRecoil, 0f, 0f);
        Quaternion shakeRot = Quaternion.Euler(jitter.y, jitter.x, 0f);

        // 5) Always apply on top of the original baseRotation

        Quaternion quaternion = baseRotation * recoilRot * shakeRot;
        recoilJoint.localRotation = quaternion;
    }



    public void ApplyRecoil()
    {
        shakeAmount = shakeIntensity;
        jitter = Random.insideUnitCircle * shakeAmount;
        targetRecoil = targetRecoil + recoilAngle;
    }
}




