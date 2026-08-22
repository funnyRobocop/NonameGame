using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class BloodFXScript : MonoBehaviour
{
    public ParticleSystem bloodFX, tinyBloodFX;
    public float minForce = 10f;
    public bool isBody = false;

    void OnJointBreak(float breakForce){
        if(!isBody){
            bloodFX.Play();
        }
        
    }
    
    void OnCollisionEnter(Collision collision){
        
        if(collision.relativeVelocity.magnitude >= minForce){
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;
        ParticleSystem tinyBlood = (ParticleSystem)Instantiate(tinyBloodFX, pos, rot) as ParticleSystem;
        tinyBlood.transform.localScale = new Vector3(.04f,.04f,.04f);
        Destroy(tinyBlood,5f);
        }
    }
}
}