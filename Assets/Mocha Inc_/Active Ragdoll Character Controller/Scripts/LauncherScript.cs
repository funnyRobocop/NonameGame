using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class LauncherScript : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject target;
    public Transform launchPosition;
    public float coolDown = 3f;
    public float currentTimer = 0f;
    public float forceMultiplier = 1f;
    public float massMultiplier = 1f;
    public float sizeMultiplier = 1f;
    public float velocity = 5f;

    public float despawnTimer = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void LaunchObject(){
        GameObject newObject = Instantiate(projectilePrefab, launchPosition);
        Rigidbody objectRB = newObject.GetComponent<Rigidbody>();

        newObject.transform.rotation = transform.rotation;
        newObject.transform.localScale = newObject.transform.localScale*sizeMultiplier;
        objectRB.mass = objectRB.mass*massMultiplier;
        objectRB.linearVelocity = newObject.transform.forward.normalized * velocity;

        Destroy(newObject,despawnTimer);

    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target.transform);
        currentTimer += Time.deltaTime;
        if(currentTimer > coolDown){
            currentTimer = 0f;
            LaunchObject();
        }
    }

    
}
}