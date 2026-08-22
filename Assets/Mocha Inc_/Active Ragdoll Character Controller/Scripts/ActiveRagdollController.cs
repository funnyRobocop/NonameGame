using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
    public class ActiveRagdollController : MonoBehaviour
{
    public PhysicsJointController[] limbs;
    public bool isLimp = false;

    public Rigidbody playerBaseBody;

    public GameObject leftArm, rightArm;

    public PlayerController player;
    public GameObject capsuleBody;
    public float maxDistance = 6f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if((transform.position - capsuleBody.transform.position).magnitude >= maxDistance){
            transform.position = capsuleBody.transform.position;
        }
    }

    public void SetGrabStrength(float strength){
        limbs[0].SetLimbStrength(strength);
        limbs[1].SetLimbStrength(strength);
        limbs[2].SetLimbStrength(strength);
    }

    public void ResetAllLimbStrength(){
        foreach(PhysicsJointController limb in limbs){
            limb.ResetLimbStrength();

        }
    }

    public void SetBodyPositionStrength(float strength){
        limbs[0].SetPositionStrength(strength);
    }
    
    public void ResetBodyPositionStrength(){
        limbs[0].ResetPositionStrength();
    }

    public void GoLimpForSeconds(float seconds){
        foreach(PhysicsJointController limb in limbs){
            limb.GoLimp();
        }
        limbs[0].SetPositionStrength(0);

        Invoke("ResetLimp",seconds);
    }

    public void ResetLimp(){
        foreach(PhysicsJointController limb in limbs){
            limb.ResetLimbStrength();
        }

        ResetBodyPositionStrength();
    }
}
}