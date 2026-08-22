using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class PlayerController : MonoBehaviour
{
    public Rigidbody mybody;
    public Animator myAnimator;
    public float jumpSpeed = 400f;
    public bool canJump = true;
    public bool isMoving = false;

    public Camera cam;


    public float currentSpeed = 1000f;
    public float turnspeed = 10f;
    public float walkSpeed = 2500f;
    public float sprintSpeed = 4000f;

    public ConfigurableJoint capsuleJoint;
    public GameObject capsuleObject;
    public bool isLimp = false;

    public bool cameraFollow = true;

    public PhysicsJointController capsuleController;
    public float sprintAnimModifier = 1.4f;


    // Start is called before the first frame update
    void Start()
    {
        mybody = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("space") && canJump){
            mybody.AddForce(transform.up * jumpSpeed);
        }

        if(Input.GetKeyDown("z")){
            if(cameraFollow){
                cameraFollow = false;
            }
            else{
                cameraFollow = true;
            }
        }

        if(Input.GetKey("left shift")){
            currentSpeed = sprintSpeed;
            
        }
        else{
            currentSpeed = walkSpeed;

        }


        if(isMoving){

        }
        if(transform.position.y <= -10f){

        }
    }

    void FixedUpdate(){
       
        if(isMoving){

        }

        if((Input.GetMouseButton(0) || Input.GetMouseButton(1)) &&  cameraFollow){
            transform.rotation  = Quaternion.Euler(cam.transform.rotation.eulerAngles.x,cam.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        else{
            transform.rotation  = Quaternion.Euler(0,transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }


        if(Input.GetKey("w")&&!isLimp){
            mybody.AddForce(transform.forward*currentSpeed*Time.fixedDeltaTime);
            if(!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && cameraFollow){
                transform.rotation  = Quaternion.Euler(transform.rotation.eulerAngles.x, cam.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);   
            }
        }
        if(Input.GetKey("s")&&!isLimp){
            mybody.AddForce(transform.forward*-currentSpeed*Time.fixedDeltaTime);
            if(!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && cameraFollow){
                transform.rotation  = Quaternion.Euler(transform.rotation.eulerAngles.x, cam.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);   
            }
        }
        if(Input.GetKey("d")&&!isLimp){
            mybody.AddForce(transform.right*currentSpeed*Time.fixedDeltaTime);
            if(!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && cameraFollow){
                transform.rotation  = Quaternion.Euler(transform.rotation.eulerAngles.x, cam.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);   
            }
        }
        if(Input.GetKey("a")&&!isLimp){
            mybody.AddForce(transform.right*-currentSpeed*Time.fixedDeltaTime);
            if(!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && cameraFollow){
                transform.rotation  = Quaternion.Euler(transform.rotation.eulerAngles.x, cam.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);   
            }
        }


        if(mybody.linearVelocity.magnitude >= 1f){
            isMoving = true;
            myAnimator.SetBool("moving", true);

        }
        else{
            isMoving = false;
            myAnimator.SetBool("moving", false);     
        }

        if(Input.GetKey("e")){
            myAnimator.SetBool("raising",true);
        }
        else{
            myAnimator.SetBool("raising",false);
        }


    }

    void OnTriggerEnter(Collider other){
        if(other.tag != "Ground"){
            return;
        }
        else{
            myAnimator.SetBool("falling",false);
            canJump = true;
            
        }
    }

    void OnTriggerStay(Collider other){
        if(other.tag != "Ground"){
            return;
        }
        else{
            myAnimator.SetBool("falling",false);
            canJump = true;
        }
    }

    void OnTriggerExit(Collider other){
        if(other.tag != "Ground"){
            return;
        }
        else{
            myAnimator.SetBool("falling",true);
            canJump = false;    
        }
    }


    public void resetJump(){
        canJump = true;
    }

    public void cantJump(){
        canJump = false;
    }
}
}