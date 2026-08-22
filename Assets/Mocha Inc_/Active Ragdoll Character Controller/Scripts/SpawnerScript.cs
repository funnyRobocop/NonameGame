using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class SpawnerScript : MonoBehaviour
{
    public GameObject spawnObjectPrefab;
    public Transform spawnlocation;
    public float despawnTime = 999f;

    public void SpawnObject(){

        GameObject spawnedObject = (GameObject)Instantiate(spawnObjectPrefab,spawnlocation) as GameObject;

        Destroy(spawnedObject, despawnTime);

    }
}
}