using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class ExplosionScript : MonoBehaviour
{
    public float explosionForce = 10f;
    public float explosionMultiplier = 1f;
    public float upwardsModifier = 10f;
    public float radius = 15f;

    public float coolDown = 2.4f;
    public bool destroyAfterCooldown=true;

    public ParticleSystem explosionFX;

    public void Explode(){
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in colliders){
            Rigidbody rb = collider.GetComponent<Rigidbody>();

            if (rb != null){
                rb.AddExplosionForce(explosionForce * explosionMultiplier, transform.position, radius, upwardsModifier);
            }
            
            if(collider.GetComponent<ActiveRagdollController>() != null){
                collider.GetComponent<ActiveRagdollController>().GoLimpForSeconds(2f);
            }
        }

        explosionFX.Play();

        if(destroyAfterCooldown){
            Destroy(gameObject,coolDown);
        }
    }
}
}