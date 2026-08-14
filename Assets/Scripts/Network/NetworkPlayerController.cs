using UnityEngine;
using Fusion;
using Unity.Cinemachine;

public class NetworkPlayerController : NetworkBehaviour
{
        private CharacterController _controller;
        private Camera _mainCamera;
        private Animator _animator;

        [SerializeField] private Transform _normalCameraTarget; 
        
        [Header("Настройки движения")]
        [SerializeField] private float speed = 6f;

        public override void Spawned()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _mainCamera = Camera.main;

            // Если принадлежит НАШЕМУ игроку (локальному клинету)
            if (HasInputAuthority)
            {
                GameObject cameraObj = GameObject.Find("PlayerNormalCamera");
                
                if (cameraObj != null)
                {
                    CinemachineCamera vCam = cameraObj.GetComponent<CinemachineCamera>();
                    if (vCam != null)
                        vCam.Target.TrackingTarget = _normalCameraTarget;
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                Vector3 inputDirection = new Vector3(data.MoveDirection.x, 0.0f, data.MoveDirection.y).normalized;

                float currentMoveSpeed = 0f;

                if (data.MoveDirection != Vector2.zero)
                {
                    Vector3 cameraForward = _mainCamera.transform.forward;
                    cameraForward.y = 0f;
                    Vector3 cameraRight = _mainCamera.transform.right;
                    cameraRight.y = 0f;

                    Vector3 targetDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

                    transform.forward = Vector3.Slerp(transform.forward, targetDirection, Runner.DeltaTime * 15f);

                    Vector3 movement = targetDirection * speed;
                    _controller.Move(movement * Runner.DeltaTime);

                    currentMoveSpeed = speed; 
                }

                if (_animator != null)
                {
                    _animator.SetFloat("Speed", currentMoveSpeed);
                    
                    _animator.SetBool("Grounded", _controller.isGrounded);
                }
            }
        }
    }