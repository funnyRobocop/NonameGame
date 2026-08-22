using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class CanisterScript : MonoBehaviour
{

    public bool isActive = false;
    public Rigidbody mybody;
    public float force = 100f;
    public float impactForce = 10f;
    public float duration = 10f;
    public GameObject vfx;

    public AudioSource audioSource;
    public AudioClip gasNoise;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isActive){
            mybody.AddForce(transform.up * force);
        }
    }

    public void StopActive(){
        isActive = false;
        vfx.SetActive(false);
        audioSource.Stop();
    }

    public void StartActive(){
        isActive = true;
        vfx.SetActive(true);
        audioSource.Play();
    }

    void OnCollisionEnter(Collision collision){
        Debug.Log("Canister impact : "+ collision.relativeVelocity.magnitude);
        if(collision.relativeVelocity.magnitude >= impactForce && !isActive){
            isActive = true;
            audioSource.Play();
            Invoke("StopActive", duration);
            vfx.SetActive(true);
        }
    }
}
}