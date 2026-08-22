using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class AiRagdollController : MonoBehaviour
{
    public AiJointController[] joints;

    public void PermaLimpAll(){
        foreach(AiJointController jointController in joints){
            if(!jointController){
                return;
            }
            jointController.Die();
        }
    }

    public void LimpAllForSeconds(float seconds){
        foreach(AiJointController jointController in joints){
            if(!jointController){
                return;
            }
            jointController.GoLimpForSeconds(seconds);
        }
    }

    public void ResetLimpAll(){
        foreach(AiJointController jointController in joints){
            if(!jointController){
                return;
            }
            jointController.ResetLimbStrength();
        }
    }
}
}