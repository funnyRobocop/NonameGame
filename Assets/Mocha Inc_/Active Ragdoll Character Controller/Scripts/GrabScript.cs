using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class GrabScript : MonoBehaviour
{
    public Rigidbody mybody;
    public bool isHolding = false;
    public GameObject heldObject;

    public float grabStrength = 100f;
    public ConfigurableJoint grabJoint;
    public float throwMultiplier = 2f;



    public bool isLeftArm = false;

    public Animator myAnimator;
    public Transform gunHolder;

    public AudioSource audioSource;
    public AudioClip grabNoise, releaseNoise;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isLeftArm){
           if(!Input.GetMouseButton(0)){
                myAnimator.SetBool("HoldingLeft",false);
                myAnimator.SetBool("Holding",false);
                dropObject();
                isHolding = false;
           }
           else{
            if(Input.GetMouseButton(1)){
                myAnimator.SetBool("Holding",true);
            }
            else{
                myAnimator.SetBool("HoldingLeft",true);
            }
           }
        }
        else{
            if(Input.GetMouseButton(1)){
                if(Input.GetMouseButton(0)){
                    myAnimator.SetBool("Holding",true);
                }
                else{
                    myAnimator.SetBool("HoldingRight",true);
                }
                
            }
            else{
                    myAnimator.SetBool("HoldingRight",false);
                    myAnimator.SetBool("Holding",false);
                    dropObject();
                    isHolding = false;
                }
        }

        if(Input.GetMouseButtonUp(1)){
             Cursor.lockState = CursorLockMode.Locked;
        }
        if(Input.GetKeyDown("f")){
            if(isHolding){
                if(heldObject.GetComponent<GunScript>()!= null){
                    heldObject.GetComponent<GunScript>().Shoot();
                }
            }
        }
    }

    public void dropObject(){
        if(heldObject){
            Destroy(grabJoint);
            heldObject.GetComponent<Rigidbody>().linearVelocity = heldObject.GetComponent<Rigidbody>().linearVelocity * throwMultiplier;
            heldObject = null;
            audioSource.PlayOneShot(releaseNoise);
        }
    }

    void OnTriggerEnter(Collider other){
        if(isHolding){
            return;
        }

        if(isLeftArm){
            if(Input.GetMouseButton(0)&&other.gameObject.GetComponent<Rigidbody>()!=null && other.gameObject.tag != "Player" && (other.gameObject.tag == "grabbable" || other.gameObject.tag == "gun")){
                if(other.gameObject.tag == "gun"){
                    other.gameObject.transform.position = gunHolder.position;
                    other.gameObject.transform.rotation = gunHolder.rotation;
                }
                myAnimator.SetBool("HoldingLeft",true);
                heldObject = other.gameObject;
                Rigidbody grabObject = other.GetComponent<Rigidbody>();
                isHolding = true;
                
                grabJoint = other.gameObject.AddComponent(typeof(ConfigurableJoint)) as ConfigurableJoint;
                grabJoint.connectedBody = mybody;
                JointDrive jointDrive = grabJoint.angularXDrive;
                jointDrive.positionSpring = grabStrength;

                grabJoint.xMotion = ConfigurableJointMotion.Locked;
                grabJoint.yMotion = ConfigurableJointMotion.Locked;
                grabJoint.zMotion = ConfigurableJointMotion.Locked;

                grabJoint.angularXMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularYMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularZMotion = ConfigurableJointMotion.Locked;

                grabJoint.anchor = new Vector3(0,0,0);

                grabJoint.breakForce = 2000f;

                grabJoint.angularXDrive = jointDrive;
                grabJoint.angularYZDrive = jointDrive;
                audioSource.PlayOneShot(grabNoise);
            }
        }
        else{
            if(Input.GetMouseButton(1)&&other.gameObject.GetComponent<Rigidbody>()!=null && other.gameObject.tag != "Player"&& (other.gameObject.tag == "grabbable" || other.gameObject.tag == "gun")){
                if(other.gameObject.tag == "gun"){
                    other.gameObject.transform.position = gunHolder.position;
                    other.gameObject.transform.rotation = gunHolder.rotation;
                }
                myAnimator.SetBool("HoldingRight",true);
                Rigidbody grabObject = other.GetComponent<Rigidbody>();
                isHolding = true;
                heldObject = other.gameObject;
                grabJoint = other.gameObject.AddComponent(typeof(ConfigurableJoint)) as ConfigurableJoint;
                grabJoint.connectedBody = mybody;
                JointDrive jointDrive = grabJoint.angularXDrive;
                jointDrive.positionSpring = grabStrength;

                grabJoint.xMotion = ConfigurableJointMotion.Locked;
                grabJoint.yMotion = ConfigurableJointMotion.Locked;
                grabJoint.zMotion = ConfigurableJointMotion.Locked;

                grabJoint.angularXMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularYMotion = ConfigurableJointMotion.Locked;
                grabJoint.angularZMotion = ConfigurableJointMotion.Locked;

                grabJoint.breakForce = 2000f;

                grabJoint.angularXDrive = jointDrive;
                grabJoint.angularYZDrive = jointDrive;

                grabJoint.anchor = new Vector3(0,0,0);
                audioSource.PlayOneShot(grabNoise);
            }
        }
    }
}
}