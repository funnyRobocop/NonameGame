using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class AiJointController : MonoBehaviour
{
    Rigidbody mybody;
    public ConfigurableJoint myjoint;
    public bool EnableLimbImpactDamage = true;
    public float minImpactForce = 5f;

    public float baseLimbStrength;
    public float LimpTime = 1f;

    public AudioSource audioSource;
    public AudioClip hurtNoise;
    
    public bool isLimp = false;
    public bool isDead = false;
    public bool isLeg = false;
    public bool isBody = false;
    public AiRagdollController ragdollController;
    public AiJointController body;
    public Animator myAnimator;
    public AiAgentController agentController;

    void Awake(){
        mybody = GetComponent<Rigidbody>();
        myjoint = GetComponent<ConfigurableJoint>();
        baseLimbStrength = myjoint.angularXDrive.positionSpring;
    }
    
    public void ResetLimbStrength(){
        if(isDead || !myjoint){
            return;
        }
        JointDrive jointdrive = myjoint.angularXDrive;
        jointdrive.positionSpring = baseLimbStrength;
        myjoint.angularXDrive = jointdrive;
        myjoint.angularYZDrive = jointdrive;

        isLimp = false;
    }

    public void SetLimbStrength(float strength){
        if(!isDead && myjoint){
            JointDrive jointdrive = myjoint.angularXDrive;
            jointdrive.positionSpring = strength;
            myjoint.angularXDrive = jointdrive;
            myjoint.angularYZDrive = jointdrive;

            baseLimbStrength = strength;
            isLimp = false;
        }
    }

    public void SetPositionStrength(float strength){
        if(!isDead && myjoint){
            JointDrive jointdrive = myjoint.angularXDrive;
            jointdrive.positionSpring = strength;
            myjoint.xDrive = jointdrive;
            myjoint.yDrive = jointdrive;
            myjoint.zDrive = jointdrive;


            isLimp = false;
        }
    }

    public void GoLimpForSeconds(float seconds){
        if(!myjoint){
            return;
        }
        JointDrive jointdrive = myjoint.angularXDrive;
        jointdrive.positionSpring = 0;
        myjoint.angularXDrive = jointdrive;
        myjoint.angularYZDrive = jointdrive;

        Invoke("ResetLimbStrength",seconds);
    }

    void OnJointBreak(){
     
       
        if(isLeg){
            float curStrength = body.myjoint.angularXDrive.positionSpring;
            float curPosStrength = body.myjoint.xDrive.positionSpring;
            body.SetLimbStrength(curStrength/2);
            body.SetPositionStrength(curPosStrength/2);
            if(curPosStrength <40f){
                myAnimator.SetBool("crawling",true);
                agentController.moveForce = 30f;
                agentController.isCrawling = true;
            }
            
        }
    }

    public void FallOff(){
        if(!myjoint){
            return;
        }
        myjoint.breakForce = 0;
        if(isLeg){
            float curStrength = body.myjoint.angularXDrive.positionSpring;
            float curPosStrength = body.myjoint.xDrive.positionSpring;
            body.SetLimbStrength(curStrength/2);
            body.SetPositionStrength(curPosStrength/2);
            if(curPosStrength <40f){
                myAnimator.SetBool("crawling",true);
                agentController.isCrawling = true;
                agentController.moveForce = 30f;
            }
            
        }
    }

    public void Die(){
        isDead = true;
        if(!myjoint){
            return;
        }
        JointDrive jointdrive = myjoint.angularXDrive;
        jointdrive.positionSpring = 0;
        myjoint.angularXDrive = jointdrive;
        myjoint.angularYZDrive = jointdrive;
        if(isBody){
            SetPositionStrength(0);
        }
    }

    void OnCollisionEnter(Collision collision){
        if(collision.relativeVelocity.magnitude  >= minImpactForce && !isLimp && !isDead){
            isLimp = true;
            GoLimpForSeconds(LimpTime);
            if(!audioSource.isPlaying){
                audioSource.PlayOneShot(hurtNoise);
            }
            
        }
    }
}
}