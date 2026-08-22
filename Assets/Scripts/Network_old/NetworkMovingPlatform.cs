using UnityEngine;
using Fusion;
using Unity.VisualScripting;

public class NetworkMovingPlatform : NetworkBehaviour
{
    [Header("Настройки вращения")]
    [SerializeField] private float rotationSpeed = 50f; 
    [SerializeField] private bool clockWise = true;      
    [SerializeField] private float lerpSpeed = 10f;     

    [Header("Центр Вращения")]
    [SerializeField] private Transform rotationCenter; 

    private Vector3 _targetPlatformVelocity = Vector3.zero;
    private Vector3 _currentPlatformVelocity = Vector3.zero;
    private NetworkPlayerController _detectedPlayer;

    private bool _isPlayerStayingThisTick = false;

    public override void Spawned()
    {
        if (rotationCenter == null) rotationCenter = transform.root;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            
            if (playerController != null && playerController.HasInputAuthority)
            {
                _detectedPlayer = playerController;
                _isPlayerStayingThisTick = true;

                Transform center = rotationCenter != null ? rotationCenter : transform;
                Vector3 upAxis = center.up;
                
                Vector3 toPlayer = other.transform.position - center.position;
                Vector3 projectedOffset = Vector3.ProjectOnPlane(toPlayer, upAxis);
                float currentRadius = projectedOffset.magnitude;

                if (currentRadius > 0.1f)
                {
                    Vector3 movementDirection = Vector3.Cross(upAxis, projectedOffset.normalized).normalized;
                    if (!clockWise) movementDirection = -movementDirection;

                    float linearVelocity = (rotationSpeed * Mathf.Deg2Rad) * currentRadius;
                    _targetPlatformVelocity = movementDirection * linearVelocity;
                }
            }
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (_detectedPlayer == null) return;

        if (!_isPlayerStayingThisTick)
        {
            _targetPlatformVelocity = Vector3.zero;
        }

        _currentPlatformVelocity = Vector3.Lerp(_currentPlatformVelocity, _targetPlatformVelocity, Runner.DeltaTime * lerpSpeed);

        if (_currentPlatformVelocity.magnitude > 0.01f)
        {
            Vector3 platformMovementThisTick = _currentPlatformVelocity * Runner.DeltaTime;
            _detectedPlayer.SetNetworkPlatformMovement(_currentPlatformVelocity);
        }
        else
        {
            _detectedPlayer = null;
        }

        _isPlayerStayingThisTick = false;
    }
}