using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class NetworkMover : NetworkBehaviour
{
    [Header("Настройки перемещения")]
    [SerializeField] private Vector3 moveOffset; 
    [SerializeField] private float speed;                             
    [SerializeField] private float timeOffset;                        

    [Header("Настройки удара ловушки")]
    [SerializeField] private float pushForce;
    [SerializeField] private float stunDuration;

    private Vector3 _startWorldPos;
    private Rigidbody _rigidbody;
    private Vector3 _lastVelocity = Vector3.zero;
    private Vector3 _lastPosition = Vector3.zero;

    // Список для защиты от многократных ударов за один проход стены
    private HashSet<NetworkPlayerController> _hitPlayers = new HashSet<NetworkPlayerController>();

    public override void Spawned()
    {
        _startWorldPos = transform.position;        
        _rigidbody = GetComponent<Rigidbody>();
        _lastPosition = transform.position;
    }

    public override void FixedUpdateNetwork()
    {
        if (_rigidbody == null) return;

        if (Runner.IsServer)
        {
            float syncedTime = Runner.SimulationTime + timeOffset;

            float pingPong = Mathf.PingPong(syncedTime * speed, 1f);
            float smoothPingPong = Mathf.SmoothStep(0f, 1f, pingPong);
            Vector3 targetWorldPosition = _startWorldPos + (moveOffset * smoothPingPong);

            Vector3 requiredVelocity = (targetWorldPosition - _rigidbody.position) / Runner.DeltaTime;
            _rigidbody.linearVelocity = requiredVelocity;

            _lastVelocity = (transform.position - _lastPosition) / Runner.DeltaTime;
            _lastPosition = transform.position;
        }
    }
}