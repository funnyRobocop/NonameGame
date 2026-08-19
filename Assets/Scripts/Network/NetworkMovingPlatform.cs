using UnityEngine;
using Fusion;
using Unity.VisualScripting;

public class NetworkMovingPlatform : NetworkBehaviour
{
    [Header("Настройки вращения")]
    [SerializeField] private float rotationSpeed = 50f; // Скорость из NetworkRotator

    [Header("Центр Вращения")]
    [SerializeField] private Transform rotationCenter;
    private NetworkPlayerController _detectedPlayer;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            
            if (playerController != null && playerController.HasInputAuthority)
            {
                _detectedPlayer = playerController;
                Debug.Log("Stay");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        _detectedPlayer = null;
                Debug.Log("OnTriggerExit");
    }

    public override void FixedUpdateNetwork()
    {
        if (_detectedPlayer == null) return;

        Vector3 upAxis = rotationCenter.up;
                
        Vector3 toPlayer = _detectedPlayer.transform.position - rotationCenter.position;
        Vector3 projectedOffset = Vector3.ProjectOnPlane(toPlayer, upAxis);
        float currentRadius = projectedOffset.magnitude;

        if (currentRadius > 0.1f)
        {
            Vector3 movementDirection = Vector3.Cross(upAxis, projectedOffset.normalized).normalized;

            float linearVelocity = (rotationSpeed * Mathf.Deg2Rad) * currentRadius;
            Vector3 platformLinearVelocity = movementDirection * linearVelocity;

            _detectedPlayer.SetNetworkPlatformMovement(platformLinearVelocity);
        }
    }
}