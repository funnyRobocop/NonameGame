using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class PhysicsJointController : MonoBehaviour
{
    public ConfigurableJoint myJoint;
    JointDrive baseDriveAngX, baseDriveAngYZ, basePositionDrive;
    public bool isLimp = false;  
    public float RagdollForce = 3f;  
    public float RagdollDuration = 1.5f;
    
    public ActiveRagdollController ragdollController;
    public bool isBody = false;
    public bool isLeg = false;

    public AudioSource audioSource;
    public AudioClip impactSound1;
    public AudioClip impactSound2;

    public PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        myJoint = GetComponent<ConfigurableJoint>();
        if(!myJoint){
            return;
        }
        baseDriveAngX = myJoint.angularXDrive;
        baseDriveAngYZ = myJoint.angularYZDrive;
        basePositionDrive = myJoint.xDrive;

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("q")){
            if(isLimp){
                ResetLimbStrength();
                player.isLimp = false;
            }
            else{
                GoLimp();
                player.isLimp = true;
                if(isBody){
                    SetPositionStrength(0);
                }
            }
        }
    }

    public void SetLimbStrength(float strength){
        if(!myJoint){
            return;
        }
        JointDrive newDrive = myJoint.angularXDrive;
        newDrive.positionSpring = strength;
        newDrive.positionDamper = baseDriveAngX.positionDamper;

        myJoint.angularXDrive = newDrive;
        myJoint.angularYZDrive = newDrive;
    }

    public void SetPositionStrength(float strength){
        if(!myJoint){
            return;
        }
        JointDrive newDrive = myJoint.xDrive;
        newDrive.positionSpring = strength;
        newDrive.positionDamper = myJoint.xDrive.positionDamper;

        myJoint.xDrive = newDrive;
        myJoint.yDrive = newDrive;
        myJoint.zDrive = newDrive;
    }

    public void ResetPositionStrength(){
        if(!myJoint){
            return;
        }
        myJoint.xDrive = basePositionDrive;
        myJoint.yDrive = basePositionDrive;
        myJoint.zDrive = basePositionDrive;

        player.isLimp = false;
        player.gameObject.transform.position = transform.position;
        player.gameObject.GetComponent<Rigidbody>().linearVelocity = GetComponent<Rigidbody>().linearVelocity;
    }

    public void ResetLimbStrength(){
        myJoint.angularXDrive = baseDriveAngX;
        myJoint.angularYZDrive = baseDriveAngYZ;


        isLimp = false;
        if(isBody){
            ResetPositionStrength();
        }
    }

    public void GoLimp(){
        SetLimbStrength(0);
        isLimp = true;

    }

    public void GoLimpForSeconds(float seconds){
        SetLimbStrength(0);
        isLimp = true;
        Invoke("ResetLimbStrength",seconds);
    }

    void OnCollisionEnter(Collision collision){
        if(collision.relativeVelocity.magnitude >= RagdollForce){
            if(isBody){
                ragdollController.GoLimpForSeconds(1.5f);
                player.isLimp = true;
                if(audioSource && !audioSource.isPlaying){
                    int i = (int)Random.Range(0,5);
                    if(i < 3){
                        audioSource.PlayOneShot(impactSound1);
                    }
                    else{
                        audioSource.PlayOneShot(impactSound2);
                    }
                    
                }
                
                return;
                
            }
            if(isLeg){
                if(audioSource && !audioSource.isPlaying){
                    int i = (int)Random.Range(0,5);
                    if(i < 3){
                        audioSource.PlayOneShot(impactSound1);
                    }
                    else{
                        audioSource.PlayOneShot(impactSound2);
                    }
                }
                ragdollController.GoLimpForSeconds(.5f);
                player.isLimp = true;
                return;
            }
            if(audioSource && !audioSource.isPlaying){
                    int i = (int)Random.Range(0,5);
                    if(i < 3){
                        audioSource.PlayOneShot(impactSound1);
                    }
                    else{
                        audioSource.PlayOneShot(impactSound2);
                    }
                }
            GoLimp();
            Invoke("ResetLimbStrength",RagdollDuration);

        }
    }

}
}