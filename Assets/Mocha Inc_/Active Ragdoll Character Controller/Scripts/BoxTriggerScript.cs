using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace ActiveRagdoll{
public class BoxTriggerScript : MonoBehaviour
{
    public UnityEvent OnBoxEnter, OnBoxLeave;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other){
        Debug.Log("enter");
        OnBoxEnter.Invoke();
    }
     public void OnTriggerExit(Collider other){
        Debug.Log("leave");
        OnBoxLeave.Invoke();
    }
}
}