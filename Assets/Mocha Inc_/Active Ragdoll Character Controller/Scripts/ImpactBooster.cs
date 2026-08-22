using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class ImpactBooster : MonoBehaviour
{
    public Rigidbody mybody;
    public float boostMultiplier = 1f;
    public float baseforce = 100f;
    public float minforce = 10f;
    public bool boostVelocity = true;
    public Rigidbody collided;
    public float velocityBoost = 5f;
    public float aiMultiplier = 3f;
    // Start is called before the first frame update
    void Awake()
    {
        mybody = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.GetComponent<Rigidbody>() == null || collision.gameObject.tag == "Player"){
            return;
        }

        if(collision.relativeVelocity.magnitude >= minforce && collision.gameObject.GetComponent<AiRagdollBody>() != null){
            collision.gameObject.GetComponent<Rigidbody>().AddForce(-(transform.position-collision.gameObject.transform.position).normalized*baseforce*boostMultiplier * aiMultiplier, ForceMode.VelocityChange);
            if(boostVelocity){
                collided = collision.gameObject.GetComponent<Rigidbody>();
                Invoke("boostvelocity",.1f);
            }
        }
        else if(collision.relativeVelocity.magnitude >= minforce && collision.gameObject.tag == "grabbable"){
            collision.gameObject.GetComponent<Rigidbody>().AddForce(-(transform.position-collision.gameObject.transform.position).normalized*baseforce*boostMultiplier, ForceMode.VelocityChange);
            if(boostVelocity){
                collided = collision.gameObject.GetComponent<Rigidbody>();
                Invoke("boostvelocity",.1f);
            }
        }
    }

    public void boostvelocity(){
        if(collided){
            collided.linearVelocity = collided.linearVelocity * velocityBoost;
        }
    }

}

}
