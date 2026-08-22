using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ActiveRagdoll{
public class DestructableScript : MonoBehaviour
{
    public float health = 100f;
    public UnityEvent onDamaged, onDestroyed;
    public GameObject[] ActivateGameObjectsOnDestroyed, DisableGameObjectsOnDestroyed;
    public bool disableColliderOnDestroyed = true;
    public bool enableCollisionDamage = false;
    public float collisionDamage = 25f;
    public float minCollisionForce = 5f;
    public float maxCollisionForce = 15f;

    bool isDestroyed = false;
    public AudioSource audioSource;
    public AudioClip DestroySound;

    public bool despawnTimerOnDeath = false;
    public GameObject despawnObject;
    public float despawnTimer = 30f;

    public void Damage(float amount){
        
        onDamaged.Invoke();
        health -= amount;

        if(health <= 0){
            isDestroyed = true;
            GetDestroyed();
        }
    }

    public void GetDestroyed(){
        audioSource.PlayOneShot(DestroySound);
        onDestroyed.Invoke();
        if(disableColliderOnDestroyed){
            GetComponent<Collider>().enabled = false;
        }

        foreach(GameObject _object in DisableGameObjectsOnDestroyed){
            _object.SetActive(false);
        }
        foreach(GameObject _object in ActivateGameObjectsOnDestroyed){
            _object.SetActive(true);
        }

        if(despawnTimerOnDeath){
            if(despawnObject){
                Destroy(despawnObject, despawnTimer);
            }
        }

    }

    void OnCollisionEnter(Collision collision){
        if(enableCollisionDamage && collision.relativeVelocity.magnitude > minCollisionForce && !isDestroyed){
            float percent = collision.relativeVelocity.magnitude / maxCollisionForce;
            float damage =  percent * collisionDamage;
            Debug.Log(gameObject.name + " Damage: " + damage);
            Damage(damage);
        }
    }
}
}