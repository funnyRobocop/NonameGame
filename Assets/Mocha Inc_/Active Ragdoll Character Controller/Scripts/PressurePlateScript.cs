using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ActiveRagdoll{
public class PressurePlateScript : MonoBehaviour
{
    public UnityEvent onPadPressed;
    public UnityEvent onPadReleased;

    public GameObject pad;
    public bool isTriggered = false;

    void Update(){
        if(pad.transform.position.y <= .05f  && !isTriggered){
           // isTriggered = true;
         //   onPadPressed.Invoke();
         //   Debug.Log("pressure");
        }
        else if(pad.transform.position.y > .05f){
         //   isTriggered = false;
          //  onPadReleased.Invoke();
        }
    }

    void OnTriggerEnter(Collider other){
        if(other.tag == "pressureplate"){
            onPadPressed.Invoke();
        }
    }

    void OnTriggerExit(Collider other){
        if(other.tag == "pressureplate"){
            onPadReleased.Invoke();
        }
    }
}
}