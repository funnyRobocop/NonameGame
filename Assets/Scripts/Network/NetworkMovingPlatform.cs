using UnityEngine;
using Fusion;

public class NetworkMovingPlatform : NetworkBehaviour
{
    [Header("Настройки вращения")]
    [SerializeField] private float rotationSpeed = 50f; // Скорость из NetworkRotator 
    [SerializeField] private float lerpSpeed = 10f;     // Скорость сглаживания

    [Header("Центр Вращения")]
    [SerializeField] private Transform rotationCenter; 

    private Vector3 _targetPlatformVelocity = Vector3.zero;
    private Vector3 _currentPlatformVelocity = Vector3.zero;
    private NetworkPlayerController _detectedPlayer;


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            
            if (playerController != null && playerController.HasInputAuthority)
            {
                _detectedPlayer = playerController;
            }
        }
    }

    private void CalculateTargetPlatformVelocity()
    {
        Vector3 upAxis = rotationCenter.up;
                
        Vector3 toPlayer = _detectedPlayer.transform.position - rotationCenter.position;
        Vector3 projectedOffset = Vector3.ProjectOnPlane(toPlayer, upAxis);
        float currentRadius = projectedOffset.magnitude;

        if (currentRadius > 0.1f)
        {
            Vector3 movementDirection = Vector3.Cross(upAxis, projectedOffset.normalized).normalized;

            float linearVelocity = (rotationSpeed * Mathf.Deg2Rad) * currentRadius;
            
            _targetPlatformVelocity = movementDirection * linearVelocity;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _targetPlatformVelocity = Vector3.zero;
            _currentPlatformVelocity = Vector3.zero;
            _detectedPlayer = null;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_detectedPlayer == null) return;

        CalculateTargetPlatformVelocity();

        _currentPlatformVelocity = Vector3.Lerp(_currentPlatformVelocity, _targetPlatformVelocity, Runner.DeltaTime * lerpSpeed);

        if (_currentPlatformVelocity.magnitude > 0.001f)
        {
            Vector3 platformMovementThisTick = _currentPlatformVelocity * Runner.DeltaTime;
            
            _detectedPlayer.SetNetworkPlatformMovement(platformMovementThisTick);

            Quaternion rotDelta = Quaternion.Euler(0, rotationSpeed * Runner.DeltaTime, 0);
            _detectedPlayer.transform.rotation = rotDelta * _detectedPlayer.transform.rotation;
        }
    }
}