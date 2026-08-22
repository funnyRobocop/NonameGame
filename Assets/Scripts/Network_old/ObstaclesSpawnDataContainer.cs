using System.Collections.Generic;
using UnityEngine;

public class ObstaclesSpawnDataContainer : MonoBehaviour
{

    public List<SpawnData> Obstacles;

    [System.Serializable]
    public struct SpawnData
    {
        public Transform point;
        public GameObject prefab;
    }
}
