using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace ActiveRagdoll{
public class DisableColliderScript : MonoBehaviour
{
    public GameObject helper;

    public GameObject[] limbs;

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject _object in limbs){
            if(_object && _object.GetComponent<Collider>()!=null){
                Physics.IgnoreCollision(_object.GetComponent<Collider>(),helper.GetComponent<Collider>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}