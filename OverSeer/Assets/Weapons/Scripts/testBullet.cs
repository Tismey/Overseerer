using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testBullet : MonoBehaviour
{
    
    private Vector3 prevt;
    public LayerMask hitlayer;
    public GameObject hitEffect;
    public float damage = 34f;
    private float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        prevt = transform.position;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        if(timer > 2f)
        {
            Destroy(gameObject);
            return;
        }
        if(Physics.Linecast(prevt, transform.position, out hit, hitlayer))
        {
            if (hit.collider.gameObject.GetComponent<AIbase>() != null)
            {   
                
                hit.collider.gameObject.GetComponent<AIbase>().health.ApplyDamage(damage);
                hit.collider.gameObject.GetComponent<AIbase>().rb.AddForce(-(prevt-transform.position)*(damage/10),ForceMode.VelocityChange);


                Instantiate(hitEffect, hit.point, Quaternion.identity);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            
        }
        prevt = transform.position;
        timer += Time.deltaTime;
    }
}
