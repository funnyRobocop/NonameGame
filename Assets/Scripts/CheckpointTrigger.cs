using UnityEngine;
using UniRx;

public class CheckpointTrigger : MonoBehaviour
{

    public BoolReactiveProperty IsReached { get; } = new (false);

    void OnTriggerEnter(Collider other)
    {
        if (!IsReached.Value && other.CompareTag("Player"))
        {
            Debug.Log($"Checkpoint reached: {gameObject.name}");
            IsReached.Value = true;
        }
    }
}
