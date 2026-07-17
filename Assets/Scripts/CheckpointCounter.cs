using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CheckpointCounter : MonoBehaviour
{

    public int ReachedCount { get; private set; }
    public BoolReactiveProperty IsFinished { get; } = new (false);

    private readonly List<CheckpointTrigger> _checkpoints = new();

    void Awake()
    {
        _checkpoints.AddRange(transform.GetComponentsInChildren<CheckpointTrigger>());

        foreach (var checkpoint in _checkpoints)
        {
            checkpoint.IsReached.Where(isCompleted => isCompleted)
            .Subscribe(_ => { HandleCheckpointReached(); })
            .AddTo(this);
        }
    }

    private void HandleCheckpointReached()
    {
        if (IsFinished.Value) return;

        ReachedCount++;

        if (ReachedCount >= _checkpoints.Count)
        {
            IsFinished.Value = true;
            Debug.Log("All checkpoints reached!");
        }
    }
}
