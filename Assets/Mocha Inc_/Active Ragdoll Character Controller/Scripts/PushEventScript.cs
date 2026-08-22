using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ActiveRagdoll{
public class PushEventScript : MonoBehaviour
{
    public UnityEvent onTriggerEnter, onTriggerLeave;
    
    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player"){
            onTriggerEnter.Invoke();
        }
    }

    void OnTriggerExit(Collider other){
        if(other.gameObject.tag == "Player"){
            onTriggerLeave.Invoke();
        }
    }
}
}