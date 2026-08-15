using UnityEngine;
using Fusion;
using Unity.Cinemachine;
using System.Collections;

public class NetworkPlayerController : NetworkBehaviour
{
        private CharacterController _controller;
        private Camera _mainCamera;
        private Animator _animator;
        
        [Header("Настройки движения")]
        [SerializeField] private float speed = 6f;
        
        [Header("Настройки Физики Сети")]
        [SerializeField] private float gravity = -15.0f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float terminalVelocity = -53.0f; // Максимальная скорость падения
        [SerializeField] private Transform _normalCameraTarget;
        
        // Синхронизируем вертикальную скорость между клиентами через атрибут Fusion 2
        [Networked] private float _verticalVelocity { get; set; }
        
        private bool _isJumpPressedPrevious = false; // Для отслеживания одиночного клика Пробела
        private bool _isSpawnReady = false; // Предохранитель для первого кадра

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
                    {
                        vCam.Target.TrackingTarget = _normalCameraTarget;
                        vCam.Target.LookAtTarget = _normalCameraTarget;
                    }
                }
            }

            StartCoroutine(DelayPhysicsAfterSpawn());
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
            if (!_isSpawnReady) return;

            if (GetInput(out NetworkInputData data))
            {
                // --- 1. РАСЧЕТ ГРАВИТАЦИИ И ПРИЖИМАНИЯ ---
                if (_controller.isGrounded)
                {
                    // Когда корова на земле, держим скорость слегка отрицательной, 
                    // чтобы она не «взлетала» на кочках и ступенях
                    if (_verticalVelocity < 0.0f)
                    {
                        _verticalVelocity = -2.0f;
                    }

                    // --- 2. ЛОГИКА ПРЫЖКА ПО СЕТИ ---
                    // Проверяем: нажат ли Пробел СЕЙЧАС, и НЕ был ли он зажат в прошлом кадре 
                    // (чтобы исключить бесконечный взлет при удержании кнопки)
                    if (data.JumpPressed && !_isJumpPressedPrevious)
                    {
                        // Математическая формула прыжка Unity: корень из (высота * -2 * гравитация)
                        _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                        
                        if (_animator != null)
                        {
                            _animator.SetBool("Jump", true); // Включаем анимацию прыжка
                        }
                    }
                }
                else
                {
                    // Сбрасываем анимацию прыжка, когда уже летим вниз
                    if (_animator != null) _animator.SetBool("Jump", false);
                }

                // Применяем гравитацию во времени, если не достигли терминальной скорости падения
                if (_verticalVelocity > terminalVelocity)
                {
                    _verticalVelocity += gravity * Runner.DeltaTime;
                }

                // Запоминаем состояние кнопки для следующего физического кадра
                _isJumpPressedPrevious = data.JumpPressed;

                // --- 3. РАСЧЕТ ГОРИЗОНТАЛЬНОГО ДВИЖЕНИЯ WASD ---
                Vector3 inputDirection = new Vector3(data.MoveDirection.x, 0.0f, data.MoveDirection.y).normalized;
                Vector3 movement = Vector3.zero;
                float currentMoveSpeed = 0f;

                if (data.MoveDirection != Vector2.zero)
                {
                    Vector3 cameraForward = _mainCamera.transform.forward;
                    cameraForward.y = 0f;
                    Vector3 cameraRight = _mainCamera.transform.right;
                    cameraRight.y = 0f;

                    Vector3 targetDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

                    transform.forward = Vector3.Slerp(transform.forward, targetDirection, Runner.DeltaTime * 15f);

                    // Считаем горизонтальный шаг
                    movement = targetDirection * speed;
                    currentMoveSpeed = speed; 
                }

                // ВАЖНО: Добавляем высчитанную вертикальную скорость гравитации/прыжка прямо в итоговый вектор
                movement.y = _verticalVelocity;

                // Двигаем Character Controller со строгим учетом сетевого шага
                _controller.Move(movement * Runner.DeltaTime);

                if (_animator != null)
                {
                    _animator.SetFloat("Speed", currentMoveSpeed);
                    
                    float motionSpeedMultiplier = (data.MoveDirection != Vector2.zero) ? 1f : 0f;
                    _animator.SetFloat("MotionSpeed", motionSpeedMultiplier);
                    
                    // Передаем статус земли, чтобы включались анимации падения Fall/FreeFall, если мы летим долго
                    _animator.SetBool("Grounded", _controller.isGrounded);
                    _animator.SetBool("FreeFall", !_controller.isGrounded && _verticalVelocity < -1f);
                }
            }
        }
    }