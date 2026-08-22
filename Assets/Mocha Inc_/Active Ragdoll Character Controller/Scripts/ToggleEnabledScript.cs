using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class ToggleEnabledScript : MonoBehaviour
{
    public GameObject myObject;
    public bool isEnabled = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Enable(){
        isEnabled = true;
        myObject.SetActive(true);
    }
    public void Disable(){
        isEnabled = false;
        myObject.SetActive(false);
    }

    public void ToggleEnabled(){
        if(isEnabled){
            isEnabled = false;
            myObject.SetActive(false);
        }
        else{
            isEnabled = true;
            myObject.SetActive(true);
        }
    }
}
}