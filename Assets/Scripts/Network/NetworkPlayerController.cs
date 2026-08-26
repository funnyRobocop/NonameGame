using UnityEngine;
using Fusion;
using Unity.Cinemachine;
using System.Collections;

namespace NonameGame
{
    public class NetworkPlayerController : NetworkBehaviour
    {
        private Rigidbody _rigidbody;
        private Animator _animator;
        private PhysicsPlayerController _characterManager;

        [SerializeField] private Transform followTarget;
        
        [Networked] private Vector3 _lastCheckpointPosition { get; set; }

        [Header("Сетевой статус финиша")]
        [Networked] public NetworkBool IsFinished { get; set; }
        [Networked] public int FinishPlace { get; set; }


        [Header("Для анимации")]
        [Networked] private float _netSpeed { get; set; }
        [Networked] private NetworkBool _netGrounded { get; set; }
        [Networked] private NetworkBool _netJumpTrigger { get; set; }
        [Networked] private NetworkBool _netDashTrigger { get; set; }
        [Networked] private NetworkBool _netStunTrigger { get; set; } 
        
        private bool _isSpawnReady = false; // Предохранитель для первого кадра

        [SerializeField] private GameObject viewPrefab;
        private NetworkVisualFollow _view;
        public NetworkPlayerRagdoll _ragdoll;

        public override void Spawned()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            _characterManager = GetComponent<PhysicsPlayerController>();

            if (viewPrefab != null)
            {
                GameObject localVisual = Instantiate(viewPrefab, transform.position, transform.rotation);
                
                _view = localVisual.GetComponent<NetworkVisualFollow>();
                if (_view != null)
                {
                    _view.InitializeFollowTarget(followTarget, _characterManager, _rigidbody);
                }

                _ragdoll = localVisual.GetComponent<NetworkPlayerRagdoll>();
                _ragdoll.Init(_characterManager);
                _ragdoll.LocalToggleRagdoll(false);
            }

            // Если принадлежит НАШЕМУ игроку (локальному клинету)
            if (HasInputAuthority)
            {
                GameObject cameraObj = GameObject.Find("PlayerNormalCamera");
                
                if (cameraObj != null)
                {
                    CinemachineCamera vCam = cameraObj.GetComponent<CinemachineCamera>();
                    if (vCam != null)
                    {
                        vCam.Target.TrackingTarget = _view._normalCameraTarget;
                        vCam.Priority = 10;
                    }
                }
            }

            if (Runner.IsServer)
            {
                _lastCheckpointPosition = transform.position;    
            }

            StartCoroutine(DelayPhysicsAfterSpawn());
            
            Debug.Log($"HasInputAuthority = {Object.HasInputAuthority}, InputAuthority = {Object.InputAuthority}, LocalPlayer = {Runner.LocalPlayer}");
        }

        private IEnumerator DelayPhysicsAfterSpawn()
        {
            // Ждем два физических кадра, чтобы CharacterController намертво закрепился на платформе
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            
            _isSpawnReady = true;
        }

        public override void FixedUpdateNetwork()
        {
            if (!_isSpawnReady || IsFinished) return;

            Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            _netSpeed = horizontalVelocity.magnitude;

            if (_characterManager != null)
                _netGrounded = _characterManager.GetGrounded(); 
        }

        /*public override void Render()
        {
            if (_animator != null)
            {
                _animator.SetFloat("Speed", _netSpeed);
                _animator.SetFloat("MotionSpeed", 10); //TODO не нужна строчка
                _animator.SetBool("Grounded", _netGrounded);
                
                if (_netJumpTrigger)
                {
                    _animator.SetBool("Jump", true);
                    if (HasInputAuthority || Runner.IsServer) _netJumpTrigger = false;
                }
                else
                {
                    _animator.SetBool("Jump", false); //TODO не нужна строчка
                }

                if (_netDashTrigger)
                {
                    _animator.SetTrigger("Dash"); 
                    
                    if (HasInputAuthority || Runner.IsServer) _netDashTrigger = false;
                }

                if (_netStunTrigger)
                {
                    _animator.SetTrigger("Stun"); 
                    
                    if (HasInputAuthority || Runner.IsServer) _netStunTrigger = false;
                }
            }
        }*/

        public void UpdateCheckpoint(Vector3 newPosition)
        {
            _lastCheckpointPosition = newPosition;
        }

        public void RespawnAtCheckpoint()
        {
            if (Runner.IsServer)
            {
                StartCoroutine(ServerRespawnRoutine());
            }
        }

        private IEnumerator ServerRespawnRoutine()
        {
            var ragdoll = GetComponent<PlayerRagdoll>();
            if (ragdoll != null)
            {
                ragdoll.ToggleRagdoll(false); 
            }

            yield return new WaitForFixedUpdate();

            //TODO Regdoll переделать
            var allRigidbodies = GetComponentsInChildren<Rigidbody>();
            foreach (var rb in allRigidbodies)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            transform.position = _lastCheckpointPosition;

            Debug.Log($"[Сеть] Физический сетевой респавн завершен успешно! Точка: {_lastCheckpointPosition}");
        }

        public void SetPlayerFinished(int place)
        {
            if (Runner.IsServer)
            {
                IsFinished = true;
                FinishPlace = place;
                
                // Включаем RPC, чтобы локальный клиент увидел UI победы
                RPC_OnLocalPlayerFinished(place);
            }
        }

        // Этот RPC сработает на ПК у конкретного игрока, который добежал до финиша
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void RPC_OnLocalPlayerFinished(int place)
        {
            Debug.Log($"[Финиш] Вы успешно финишировали! Ваше место: {place}");
            
            // Включаем анимацию празднования/Idle в аниматоре, если она есть
            if (_animator != null)
            {
            }
        }
    }
}