using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class MatchRotation : MonoBehaviour
{
    public Transform target;
    public ConfigurableJoint thisJoint;
    public bool mirror = false;

    void Start(){
        thisJoint = GetComponent<ConfigurableJoint>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if(!thisJoint || !target){
            return;
        }
        if(mirror){
            thisJoint.targetRotation = Quaternion.Inverse(target.localRotation);
        }
        else{
            thisJoint.targetRotation = target.localRotation;
        }

    }
}
}