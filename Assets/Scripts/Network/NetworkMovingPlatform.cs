using UnityEngine;
using Fusion;

public class NetworkMovingPlatform : NetworkBehaviour
{
    [Header("Настройки вращения")]
    [SerializeField] private float rotationSpeed = 50f; // Скорость из NetworkRotator
    [SerializeField] private bool clockWise = true;      
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
            if (!clockWise) movementDirection = -movementDirection;

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
            // ВАЖНО: Умножаем скорость на Runner.DeltaTime СТРОГО в момент передачи в игрока!
            // Это гарантирует математическую точность сетевого кадра
            Vector3 platformMovementThisTick = _targetPlatformVelocity * Runner.DeltaTime;
            
            _detectedPlayer.SetNetworkPlatformMovement(platformMovementThisTick);

                        float directionSign = clockWise ? 1f : -1f;
            Quaternion rotDelta = Quaternion.Euler(0, rotationSpeed * directionSign * Runner.DeltaTime, 0);
            _detectedPlayer.transform.rotation = rotDelta * _detectedPlayer.transform.rotation;
        }
    }
}