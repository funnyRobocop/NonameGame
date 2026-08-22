using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class RadioScript : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] songList;
    public int currentSong = 0;
    public bool isPlaying = false;

    public void NextSong(){
        currentSong += 1;
        if(currentSong >= songList.Length){
            currentSong = 0;
        }
        audioSource.Stop();
        audioSource.PlayOneShot(songList[currentSong]);
    }

    public void ToggleEnable(){
        if(isPlaying){
            isPlaying = false;
            audioSource.Stop();
        }
        else{
            isPlaying = true;
            audioSource.PlayOneShot(songList[currentSong]);
        }
    }

    void Update(){
        if(isPlaying){
            if(!audioSource.isPlaying){
                NextSong();
            }
        }
    }
}
}