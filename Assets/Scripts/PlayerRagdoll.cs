using UnityEngine;
using Unity.Cinemachine; // Подключаем Cinemachine

public class PlayerRagdoll : MonoBehaviour
{
    [Header("Настройки Камер Cinemachine")]
    [SerializeField] private CinemachineCamera normalCamera;  // Сюда перетащите PlayerFollowCamera
    [SerializeField] private CinemachineCamera ragdollCamera; // Сюда перетащите RagdollFollowCamera
    
    [Header("Кости")]
    [SerializeField] private Transform ragdollHips;          // Сюда кость mixamorig:Hips

    private CharacterController _controller;
    private Animator _animator;
    private Rigidbody[] _ragdollRigidbones;
    private Collider[] _ragdollColliders;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        
        _ragdollRigidbones = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = GetComponentsInChildren<Collider>();

        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool isRagdoll)
    {
        // Отключаем контроллер и анимации при падении
        if (_controller != null) _controller.enabled = !isRagdoll;
        if (_animator != null) _animator.enabled = !isRagdoll;
        
        var movementScript = GetComponent<ThirdPersonController>();
        if (movementScript != null) movementScript.enabled = !isRagdoll;

        foreach (var rb in _ragdollRigidbones) rb.isKinematic = !isRagdoll;
        foreach (var col in _ragdollColliders)
        {
            if (col.gameObject != this.gameObject) col.enabled = isRagdoll;
        }

        // ВАЖНО: Управляем приоритетами камер для идеального переключения
        if (normalCamera != null && ragdollCamera != null)
        {
            if (isRagdoll)
            {
                // Включаем камеру рэгдолла, делая её приоритет максимальным
                normalCamera.Priority = 5;
                ragdollCamera.Priority = 15;
            }
            else
            {
                // Возвращаем приоритет обычной камере персонажа
                normalCamera.Priority = 15;
                ragdollCamera.Priority = 5;
            }
        }
    }

    public void ApplyRagdollImpulse(Vector3 forceDirection, float forceMagnitude, float timeToStandUp)
    {
        ToggleRagdoll(true);

        if (_ragdollRigidbones.Length > 0)
        {
            foreach (var rb in _ragdollRigidbones)
            {
                rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
            }
        }

        Invoke(nameof(StandUp), timeToStandUp);
    }

    private void StandUp()
    {
        if (ragdollHips != null)
        {
            Vector3 targetPosition = ragdollHips.position;
            targetPosition.y += 0.1f; 
            transform.position = targetPosition;
        }

        ToggleRagdoll(false);
    }
}
