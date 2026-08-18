using UnityEngine;
using Fusion;
using Unity.Cinemachine;
using System.Collections;

public class NetworkPlayerController : NetworkBehaviour
{
    private CharacterController _controller;
    private PlayerRagdoll _ragdoll;
    private Camera _mainCamera;
    private Animator _animator;
    
    [Header("Настройки движения")]
    [SerializeField] private float speed;
    
    [Header("Настройки Физики Сети")]
    [SerializeField] private float gravity;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float terminalVelocity; // Максимальная скорость падения
    [SerializeField] private Transform _normalCameraTarget;
    [SerializeField] private float antiImpactForce = 5f; // Скорость затухания отскока
    [SerializeField] private float impactThreshold = 0.2f;
    [SerializeField] float pushPower = 7f; 
    
    [Networked] private float _verticalVelocity { get; set; }
    [Networked] private Vector3 _lastCheckpointPosition { get; set; }
    [Networked] private Vector3 _impactForce { get; set; }

    [Header("Сетевой статус финиша")]
    [Networked] public NetworkBool IsFinished { get; set; }
    [Networked] public int FinishPlace { get; set; }
    
    private bool _isJumpPressedPrevious = false; // Для отслеживания одиночного клика Пробела
    private bool _isSpawnReady = false; // Предохранитель для первого кадра
    private Vector3 _platformMovement = Vector3.zero;

    public override void Spawned()
    {
        _controller = GetComponent<CharacterController>();
        _ragdoll = GetComponent<PlayerRagdoll>();
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
                    vCam.Priority = 10;
                    //vCam.Target.LookAtTarget = _normalCameraTarget;
                }
            }
        }

        if (Runner.IsServer)
        {
            // По умолчанию точка возрождения — это место, где игрок заспавнился на старте
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
        if (!_isSpawnReady) return;

        // Если игрок уже финишировал — полностью блокируем считывание WASD, кнопок и прыжков
        if (IsFinished)
        {
            // Принудительно держим скорость нулевой, но сохраняем прижимание к земле
            if (_controller != null && _controller.isGrounded)
            {
                _verticalVelocity = -3.3f;
                _controller.Move(new Vector3(0, _verticalVelocity, 0) * Runner.DeltaTime);
            }
            return; 
        }

        if (GetInput(out NetworkInputData data))
        {
            // --- 1. РАСЧЕТ ГРАВИТАЦИИ И ПРИЖИМАНИЯ ---
            if (_controller.isGrounded)
            {
                // Когда корова на земле, держим скорость отрицательной, 
                // чтобы она не «взлетала» на кочках и ступенях
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -3.3f;
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

            // --- ОБРАБОТКА СЕТЕВОГО ОТСКОКА ---
            if (_impactForce.magnitude > impactThreshold)
            {
                // Прибавляем вектор отскока прямо к базовому движению WASD
                movement += _impactForce;
                
                // Плавно тушим силу отскока с каждым сетевым кадром (Runner.DeltaTime вместо Time.deltaTime)
                _impactForce = Vector3.Lerp(_impactForce, Vector3.zero, Runner.DeltaTime * antiImpactForce);
            }
            else
            {
                _impactForce = Vector3.zero;
            }

            // Добавляем высчитанную вертикальную скорость гравитации/прыжка в итоговый вектор
            movement.y = _verticalVelocity;

            Vector3 finalMovement = movement;
            finalMovement *= Runner.DeltaTime;
            finalMovement += _platformMovement;

            // Двигаем Character Controller со строгим учетом сетевого шага
            if (_isSpawnReady && _controller.enabled)
            {
                _controller.Move(finalMovement);
            }
            
             _platformMovement = Vector3.zero;

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

    // Метод обновления чекпоинта (вызывается только на сервере)
    public void UpdateCheckpoint(Vector3 newPosition)
    {
        _lastCheckpointPosition = newPosition;
    }

    // Этот метод вызывается на сервере (внутри NetworkKillzone)
    public void RespawnAtCheckpoint()
    {
        if (Runner.IsServer)
        {
            // Запускаем безопасную цепочку возрождения через корутину
            StartCoroutine(ServerRespawnRoutine());
        }
    }

    private IEnumerator ServerRespawnRoutine()
    {
        // 1. Принудительно выключаем рэгдолл через готовый RPC на всех экранах
        var ragdoll = GetComponent<NetworkPlayerRagdoll>();
        if (ragdoll != null)
        {
            // Вызываем RPC_StandUp или метод принудительного выключения физики костей
            // Чтобы кости перестали жить своей жизнью до телепортации
            ragdoll.LocalToggleRagdoll(false); 
        }

        // Ждем один физический кадр, чтобы Unity успела переключить isKinematic на костях скелета
        yield return new WaitForFixedUpdate();

        // 2. Полностью обнуляем физическую скорость на корне и костях, чтобы убрать инерцию падения
        _verticalVelocity = 0f;
        var allRigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRigidbodies)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 3. Отключаем CharacterController, чтобы он разрешил мгновенную смену координат
        if (_controller != null) _controller.enabled = false;

        // 4. Переносим корову на сохраненный чекпоинт
        transform.position = _lastCheckpointPosition;

        // Даем кадру Unity зафиксировать трансформ
        yield return new WaitForFixedUpdate();

        // 5. Включаем CharacterController обратно
        if (_controller != null) _controller.enabled = true;

        Debug.Log($"[Сеть] Физический респавн рэгдолла завершен. Точка: {_lastCheckpointPosition}");
    }

    public void ApplyNetworkKnockback(Vector3 direction, float force)
    {
        // Во Fusion изменять [Networked] переменные напрямую разрешено только Серверу (StateAuthority)
        // или владельцу ввода (InputAuthority), если включен специальный режим. 
        // Самый надежный способ для прототипа — прикладывать силу на стороне того, кто управляет телом
        if (HasInputAuthority || Runner.IsServer)
        {
            _impactForce += direction * force;
        }
    }

    public void SetNetworkPlatformMovement(Vector3 movement)
    {
        _platformMovement = movement;
    }

    public void ApplyNetworkTrampolineBounce(float force)
    {
        // Менять [Networked] параметры во Fusion разрешено только владельцу ввода или серверу
        if (HasInputAuthority || Runner.IsServer)
        {
            // Мы напрямую перезаписываем вертикальную скорость! 
            // Вместо плавного прибавления, мы даем резкий мощный пинок строго вверх
            _verticalVelocity = force;

            // Включаем анимацию прыжка/полета в аниматоре
            if (_animator != null)
            {
                _animator.SetBool("Jump", true);
                _animator.SetBool("FreeFall", false);
            }
        }
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Проверяем, есть ли у объекта твердое физическое тело
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        // Игнорируем, если игрок наступил на мяч сверху
        if (hit.moveDirection.y < -0.3f) return;

        // Ищем сетевой объект на мяче
        NetworkObject netObj = body.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            // Направление и сила удара
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z).normalized;

            // Силу к независимому Rigidbody прикладывает строго СЕРВЕР (StateAuthority)
            if (Runner.IsServer)
            {
                // Если вы Хост/Сервер — пинаем мяч напрямую
                body.AddForce(pushDir * pushPower, ForceMode.Impulse);
            }
            else if (HasInputAuthority)
            {
                // Если вы обычный клиент — отправляем быстрый RPC-запрос серверу с просьбой пнуть этот конкретный мяч
                RPC_RequestPushObject(netObj, pushDir * pushPower);
            }
        }
    }

    // Сетевой RPC-метод для отправки пинка от клиента на сервер
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestPushObject(NetworkObject targetNetObj, Vector3 force)
    {
        if (targetNetObj != null)
        {
            Rigidbody rb = targetNetObj.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                // Сервер послушно прикладывает силу к мячу на своем экране
                rb.AddForce(force, ForceMode.Impulse);
                Debug.Log($"[Сервер] Выполнен RPC пинок объекта: {targetNetObj.name}");
            }
        }
    }

    public void SetPlayerFinished(int place)
    {
        if (Runner.IsServer)
        {
            IsFinished = true;
            FinishPlace = place;
            
            // Включаем RPC, чтобы локальный клиент увидел UI победы и у него отключился ввод
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