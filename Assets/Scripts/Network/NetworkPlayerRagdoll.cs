using UnityEngine;
using Fusion;
using Unity.Cinemachine;
using System.Collections;
using Zenject;

public class NetworkPlayerRagdoll : NetworkBehaviour
{

    [Header("Кости и Настройки")]
    [SerializeField] private Transform ragdollHips; 
    [SerializeField] private LayerMask groundLayer;  
    [SerializeField] private float standUpDistance = 0.3f; 

    private CameraSwitcher _cameraSwitcher;
    private CharacterController _controller;
    private Animator _animator;
    private Rigidbody[] _ragdollRigidbones;
    private Collider[] _ragdollColliders;
    private Rigidbody _hipsRigidbody;
    
    private bool _isRagdollActive = false;
    private Coroutine _groundCheckCoroutine;
    public Transform HipsTransform => ragdollHips;

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

        if (ragdollHips != null)
        {
            _hipsRigidbody = ragdollHips.GetComponent<Rigidbody>();
        }

        // При самом старте в Awake выключаем физику костей локально
        LocalToggleRagdoll(false);
    }

    // Атрибут [Rpc] говорит Fusion: когда этот метод вызывается, выполни его на ВСЕХ компьютерах в сети
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ApplyRagdollImpulse(Vector3 forceDirection, float forceMagnitude, CinemachineCamera _camera = null)
    {
        // 1. Включаем режим рэгдолла у всех на экранах
        LocalToggleRagdoll(true);

        // 2. Прикладываем физический импульс к костям (это сработает локально у каждого клиента)
        if (_ragdollRigidbones.Length > 0)
        {
            foreach (var rb in _ragdollRigidbones)
            {
                rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
            }
        }

        // 3. Отслеживание земли для подъема запускает ТОЛЬКО владелец этого персонажа
        if (HasInputAuthority)
        {
            if (_camera != null) _cameraSwitcher.SwitchRagdollCameras(_camera);
            if (_groundCheckCoroutine != null) StopCoroutine(_groundCheckCoroutine);
            _groundCheckCoroutine = StartCoroutine(CheckForGroundLanding());
        }
    }

    private void LocalToggleRagdoll(bool isRagdoll)
    {
        _isRagdollActive = isRagdoll;

        if (_controller != null) _controller.enabled = !isRagdoll;
        if (_animator != null) _animator.enabled = !isRagdoll;
        
        var movementScript = GetComponent<NetworkPlayerController>();
        if (movementScript != null) movementScript.enabled = !isRagdoll;

        foreach (var rb in _ragdollRigidbones) rb.isKinematic = !isRagdoll;
        foreach (var col in _ragdollColliders)
        {
            if (col.gameObject != this.gameObject) col.enabled = isRagdoll;
        }

        if (HasInputAuthority)
        {
            if (isRagdoll)
            {           
                if (_groundCheckCoroutine != null) StopCoroutine(_groundCheckCoroutine);
                _groundCheckCoroutine = StartCoroutine(CheckForGroundLanding());
            }
            else
            {
                if (_cameraSwitcher != null)
                    _cameraSwitcher.ResetRagdollCameras();
            }
        }
    }

    private IEnumerator CheckForGroundLanding()
    {
        yield return new WaitForSeconds(0.4f);

        while (_isRagdollActive)
        {
            if (ragdollHips != null && _hipsRigidbody != null)
            {
                Ray ray = new Ray(ragdollHips.position, Vector3.down);
                if (Physics.Raycast(ray, standUpDistance + 0.5f, groundLayer))
                {
                    if (_hipsRigidbody.linearVelocity.magnitude < 1.5f) 
                    {
                        // Когда владелец ввода видит, что приземлился — отправляем RPC команду "Встать" для ВСЕХ
                        RPC_StandUp();
                        yield break;
                    }
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    // Сетевой RPC-метод для синхронного подъема на ноги
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_StandUp()
    {
        if (_groundCheckCoroutine != null) StopCoroutine(_groundCheckCoroutine);

        if (ragdollHips != null)
        {
            Vector3 targetPosition = ragdollHips.position;
            
            RaycastHit hit;
            if (Physics.Raycast(ragdollHips.position, Vector3.down, out hit, 3f, groundLayer))
            {
                targetPosition.y = hit.point.y + 0.05f;
            }
            else
            {
                targetPosition.y += 0.1f;
            }

            // Переносим корень объекта вслед за улетевшим тазом у каждого игрока
            transform.position = targetPosition;

            Vector3 forwardDirection = ragdollHips.forward;
            forwardDirection.y = 0; 
            if (forwardDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forwardDirection);
            }
        }

        LocalToggleRagdoll(false);
    }
}
