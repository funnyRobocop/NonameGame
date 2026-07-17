using System.Collections.Generic;
using UnityEngine;

public class CheckpointCounter : MonoBehaviour
{

    public event System.Action OnFinished;

    public int ReachedCount { get; private set; }
    public bool IsFinished { get; private set; }

    private List<CheckpointTrigger> _checkpoints = new();

    private void Awake()
    {
        _checkpoints.AddRange(transform.GetComponentsInChildren<CheckpointTrigger>());

        foreach (var checkpoint in _checkpoints)
        {
            checkpoint.OnReached += HandleCheckpointReached;
        }
    }

    void OnDestroy()
    {
        foreach (var checkpoint in _checkpoints)
        {
            checkpoint.OnReached -= HandleCheckpointReached;
        }
    }

    private void HandleCheckpointReached()
    {
        if (IsFinished) return;

        ReachedCount++;

        if (ReachedCount >= _checkpoints.Count)
        {
            IsFinished = true;
            OnFinished?.Invoke();
            Debug.Log("All checkpoints reached!");
        }
    }
}
