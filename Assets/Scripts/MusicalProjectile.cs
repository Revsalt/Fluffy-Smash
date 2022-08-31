using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicalProjectile : MonoBehaviour
{
    Vector3 ContactPoint;
    float mindistance = 10f;
    public Rigidbody rigidBody;
    public float force = 1000;
    public GameObject Playerobject;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody.AddForce(transform.forward * force);
    } 


    // Update is called once per frame
    void Update()
    {
        OnDrawGizmos();
    }
    
    private void OnCollisionEnter(Collision collision)
    {       
        ContactPoint contact = collision.contacts[0];
        ContactPoint = contact.point;
        Explode();
        Destroy(gameObject);
       
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Playerobject.transform.position, this.transform.position);
    }
    void Explode()
    {
       // Playerobject.GetComponent<beetcatin>().Force(CalculateDirection(), calculateforceimpact());
    }
    public float calculateforceimpact()
    {
        float distance = Vector3.Distance(ContactPoint, Playerobject.transform.position);        
        if (distance <= mindistance)
        {
            return 1/distance;
        }
        else
        {
            return 0;
        }       
    }
    public Vector3 CalculateDirection()
    {
        return Playerobject.transform.position - ContactPoint;
    }
    public Vector3 PointOfContact()
    {
        return ContactPoint;
    }
}
