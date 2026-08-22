using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ActiveRagdoll{
public class AiAgentController : MonoBehaviour
{
    public NavMeshAgent myAgent;
    public Transform target;
    public bool isChasing = true;
    public float moveForce = 200f;
    Rigidbody mybody;
    public Animator myAnimator;
     bool attacking = false;
     public float attackCooldown = 1f;
     public float attackDistance = 2f;
     public float minKnockbackForce = 50f;
     public float maxKnockbackForce = 100f;

    public bool isCrawling = false;
    // Start is called before the first frame update
    void Awake()
    {
        //myAgent = GetComponent<NavMeshAgent>();
        //myAgent.updatePosition = false;
        mybody = GetComponent<Rigidbody>();
        GameObject[] playertags = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject playertag in playertags){
            if(playertag.name == "PlayerCapsule" || playertag.name == "Belly"){
                target = playertag.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(isChasing && target != null){
            myAgent.destination = target.position;
        }
        else{
            myAgent.destination = transform.position;
        }
        */

        if(isChasing && target != null){
            transform.LookAt(target);
        }
        if(mybody.linearVelocity.magnitude >= .5f){
            myAnimator.SetBool("moving",true);
        }
        else{
            myAnimator.SetBool("moving",false);
        }

        if(isChasing && !attacking){
            if((transform.position - target.position).magnitude <= attackDistance){
                attacking = true;
                Debug.Log("ATTACK");
                Invoke("ResetAttacking",attackCooldown);
                myAnimator.Play("ZombieAttack");
                myAnimator.SetBool("attack",true);
                Debug.Log("DISTANCE " + (transform.position - target.position).magnitude);
                if(target.gameObject.GetComponent<Rigidbody>() != null && !isCrawling){
                    float force = Random.Range(minKnockbackForce,maxKnockbackForce);
                    target.gameObject.GetComponent<Rigidbody>().AddForce((target.position - transform.position).normalized * force,ForceMode.Impulse);
                    if(target.gameObject.GetComponent<ActiveRagdollController>() != null){
                        float rand = Random.Range(0,4);
                        if(rand > 1){
                            target.gameObject.GetComponent<PhysicsJointController>().GoLimpForSeconds(1f);
                        }
                        
                    }
                }

                
            }
        }
    }

    void FixedUpdate(){
        if(isChasing && target != null){
            mybody.AddForce(mybody.transform.forward.normalized * moveForce);
        }
    }

    public void ResetAttacking(){
        attacking = false;
        myAnimator.SetBool("attack",false);
    }


    
}
}