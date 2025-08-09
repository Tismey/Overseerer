using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInControl : AIState
{
    public Camera cam;
    public int numberOfRays = 10; // Nombre de raycasts
    public float arcAngle = 90f;  // Angle de l'arc en degrés
    public float rayDistance = 5f; // Distance des raycasts
    public float shoveCooldown = 1f;
    private float shoveTimer = 0f;
    private PlayerMouvement playerMouvement;

    public override void Setup()
    {
        this.Animator.SetBool("Moving", true);
        this.ai.lockRoot.Lock();
        this.ai.canMove = true;
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.None;
        playerMouvement = ai.GetComponent<PlayerMouvement>();

    }
    public override void act()
    {
        //do nothing
        //this.Animator.SetBool("Idle",true);
        shoveTimer += Time.deltaTime;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Vector3.Distance(cam.transform.position, ai.transform.position);  // distance entre la caméra et l'objet
        Vector3 worldPosition = cam.ScreenToWorldPoint(mousePos);
        Vector3 viewDirection = new Vector3(ai.transform.position.x + playerMouvement.viewInput.x, ai.transform.position.y, ai.transform.position.z + playerMouvement.viewInput.y);
        ai.LookTowards(viewDirection);
        if (playerMouvement.ShoveInput && shoveTimer > shoveCooldown)
        {
            CastArcRays();
            
            //this.Animator.SetTrigger("Shove");
        }
        if (playerMouvement.ShootInput && ai.weapon != null)
        {
            ai.weapon[ai.WeaponSelect].Shoot(cam.transform.forward,ai.eyePosition.position); ;
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
}




