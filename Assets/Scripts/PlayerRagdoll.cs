using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class PlayerRagdoll : MonoBehaviour
{
    [Header("Камеры Cinemachine")]
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera ragdollCamera;
    
    [Header("Кости и Настройки")]
    [SerializeField] private Transform ragdollHips;
    [SerializeField] private LayerMask groundLayer; 
    [SerializeField] private float standUpDistance = 0.1f; // Дистанция до земли для подъема

    private CharacterController _controller;
    private Animator _animator;
    private Rigidbody[] _ragdollRigidbones;
    private Collider[] _ragdollColliders;
    private Rigidbody _hipsRigidbody;
    
    private bool _isRagdollActive;
    private Coroutine _groundCheckCoroutine;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        
        _ragdollRigidbones = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = GetComponentsInChildren<Collider>();

        if (ragdollHips != null)
        {
            _hipsRigidbody = ragdollHips.GetComponent<Rigidbody>();
        }

        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool isRagdoll)
    {
        _isRagdollActive = isRagdoll;

        if (_controller != null) _controller.enabled = !isRagdoll;
        if (_animator != null) _animator.enabled = !isRagdoll;
        
        var movementScript = GetComponent<ThirdPersonController>();
        if (movementScript != null) movementScript.enabled = !isRagdoll;

        foreach (var rb in _ragdollRigidbones) rb.isKinematic = !isRagdoll;
        foreach (var col in _ragdollColliders)
        {
            if (col.gameObject != this.gameObject) col.enabled = isRagdoll;
        }

        if (normalCamera != null && ragdollCamera != null)
        {
            if (isRagdoll)
            {
                normalCamera.Priority = 5;
                ragdollCamera.Priority = 15;
            }
            else
            {
                normalCamera.Priority = 15;
                ragdollCamera.Priority = 5;
            }
        }

        // Если рэгдолл включился — запускаем корутину отслеживания земли
        if (isRagdoll)
        {
            if (_groundCheckCoroutine != null) StopCoroutine(_groundCheckCoroutine);
            _groundCheckCoroutine = StartCoroutine(CheckForGroundLanding());
        }
    }

    public void ApplyRagdollImpulse(Vector3 forceDirection, float forceMagnitude)
    {
        ToggleRagdoll(true);

        if (_ragdollRigidbones.Length > 0)
        {
            foreach (var rb in _ragdollRigidbones)
            {
                rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
            }
        }
    }

    private IEnumerator CheckForGroundLanding()
    {
        yield return new WaitForSeconds(0.1f);

        while (_isRagdollActive)
        {
            if (ragdollHips != null && _hipsRigidbody != null)
            {
                // Пускаем физический луч от таза строго вниз
                Ray ray = new Ray(ragdollHips.position, Vector3.down);
                
                // Проверяем: близко ли земля И успокоилось ли физическое тело (скорость падения упала)
                if (Physics.Raycast(ray, standUpDistance, groundLayer))
                {
                    // Проверяем вектор скорости таза.
                    if (_hipsRigidbody.linearVelocity.magnitude < 1f) 
                    {
                        StandUp();
                        yield break;
                    }
                }
            }
            
            yield return new WaitForFixedUpdate();
        }
    }

    private void StandUp()
    {
        if (_groundCheckCoroutine != null) StopCoroutine(_groundCheckCoroutine);

        if (ragdollHips != null)
        {
            var targetPosition = ragdollHips.position;

            if (Physics.Raycast(ragdollHips.position, Vector3.down, out var hit, 3f, groundLayer))
            {
                targetPosition.y = hit.point.y + 0.05f;
            }
            else
            {
                targetPosition.y += 0.1f;
            }

            transform.position = targetPosition;

            var forwardDirection = Vector3.forward;
            //forwardDirection.y = 0; // Нам нужен только горизонтальный поворот
            if (forwardDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forwardDirection);
            }
        }

        ToggleRagdoll(false);
    }
}
