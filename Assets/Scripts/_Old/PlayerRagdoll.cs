using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using Unity.VisualScripting;
using Zenject;

public class PlayerRagdoll : MonoBehaviour
{
    
    [SerializeField] private Transform _ragdollHips;
    [SerializeField] private LayerMask _groundLayer; 
    [SerializeField] private float _standUpDistance; // Дистанция до земли для подъема

    private CharacterController _controller;

    private CameraSwitcher _cameraSwitcher;
    private Animator _animator;
    private Rigidbody[] _ragdollRigidbones;
    private Collider[] _ragdollColliders;
    private Rigidbody _hipsRigidbody;
    
    private bool _isRagdollActive;
    private Coroutine _groundCheckCoroutine;

    public Transform HipsTransform => _ragdollHips;

    [Inject]
    public void Construct(CameraSwitcher cameraSwitcher)
    {
        _cameraSwitcher = cameraSwitcher;
    }
    
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        
        _ragdollRigidbones = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = GetComponentsInChildren<Collider>();

        if (_ragdollHips != null)
        {
            _hipsRigidbody = _ragdollHips.GetComponent<Rigidbody>();
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

        if (isRagdoll)
        {           
            if (_groundCheckCoroutine != null) StopCoroutine(_groundCheckCoroutine);
            _groundCheckCoroutine = StartCoroutine(CheckForGroundLanding());
        }
        else
        {
            if (_cameraSwitcher != null)
                _cameraSwitcher.SwitchOffAllRagdollCameras();
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
            if (_ragdollHips != null && _hipsRigidbody != null)
            {
                Ray ray = new Ray(_ragdollHips.position, Vector3.down);
                
                if (Physics.Raycast(ray, _standUpDistance, _groundLayer))
                {
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

        if (_ragdollHips != null)
        {
            var targetPosition = _ragdollHips.position;

            if (Physics.Raycast(_ragdollHips.position, Vector3.down, out var hit, 3f, _groundLayer))
            {
                targetPosition.y = hit.point.y + 0.05f;
            }
            else
            {
                targetPosition.y += 0.1f;
            }

            transform.position = targetPosition;

            /*var forwardDirection = Vector3 ;
            //forwardDirection.y = 0; // Нам нужен только горизонтальный поворот
            if (forwardDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forwardDirection);
            }*/
        }

        ToggleRagdoll(false);
    }
}
