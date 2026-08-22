using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class ImpactNoise : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public float impactForce;
    public bool randomVolumeEnabled = true;
    public float MaxVolumeForce = 15f;
    public float baseVolume =1f;
    void Awake(){
        baseVolume = audioSource.volume;
    }

    void OnCollisionEnter(Collision collision){
        if(collision.relativeVelocity.magnitude >= impactForce){
            if(!audioSource.isPlaying){
                int i = (int)Random.Range(0,audioClips.Length-1);
                if(randomVolumeEnabled){
                    float newVolume = collision.relativeVelocity.magnitude / MaxVolumeForce;
                    Debug.Log("AudioSource: newVolume: " + newVolume+ " magnitude: " + collision.relativeVelocity.magnitude);
                    audioSource.volume = newVolume;
                }


                audioSource.PlayOneShot(audioClips[i]);
            }
        }
    }

}
}