using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class GunScript : MonoBehaviour
{
    public Rigidbody mybody;
    public GameObject projectilePrefab;
    public Transform spawnLocation;
    public float projectileForce = 10f;
    public float coolDown = 1f;
    public float timeSinceLastShot = 0f;
    public bool isHeld = false;
    public float kickBackForce = 1f;
    public ParticleSystem muzzleFX;
    public AudioSource audioSource;
    public AudioClip pistolShot1;
    public float bulletDespawnTime = 60f;

    void Update(){
        timeSinceLastShot += Time.deltaTime;
    }

    public void Shoot(){
        if(timeSinceLastShot >= coolDown){
            timeSinceLastShot = 0;
            GameObject bullet = Instantiate(projectilePrefab) as GameObject;
            bullet.transform.position = spawnLocation.position;
            bullet.transform.rotation = spawnLocation.rotation;
            Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();
            bulletRB.AddForce(bulletRB.transform.forward.normalized * projectileForce, ForceMode.Impulse);
            mybody.AddForce(-transform.forward.normalized * kickBackForce, ForceMode.Impulse);
            mybody.AddForce(transform.up.normalized * (kickBackForce/1.5f), ForceMode.Impulse);
            
            muzzleFX.Play();
            
            audioSource.PlayOneShot(pistolShot1);
            Destroy(bullet,bulletDespawnTime);
        }
    }

}
}