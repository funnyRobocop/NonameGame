using System;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{

    public event Action OnReached;
    public bool IsReached { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        if (IsReached) return;

        if (other.CompareTag("Player"))
        {
            IsReached = true;
            OnReached?.Invoke();

            Debug.Log("Checkpoint reached!");          
        }
    }
}
