using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public List<Transform> Points;

    public Transform GetNext()
    {
        return Points[0]; //TODO
    }
}
