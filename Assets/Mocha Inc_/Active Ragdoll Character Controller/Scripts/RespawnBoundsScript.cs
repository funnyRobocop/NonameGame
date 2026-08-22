using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class RespawnBoundsScript : MonoBehaviour
{
    public Transform respawnPosition;
    public GameObject playerBase;
    public GameObject playerCapsule;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OntriggerEnter(Collider other){
        if(other.gameObject.tag == "Player"){
            playerCapsule.transform.position = respawnPosition.position;
            playerBase.transform.position = respawnPosition.position;
        }
    }
}
}