using UnityEngine;
using Fusion;
using Unity.Cinemachine;
using System.Collections;

public class NetworkPlayerController : NetworkBehaviour
{
    private NetworkCharacterController _networkController;
    private PlayerRagdoll _ragdoll;
    private Camera _mainCamera;
    private Animator _animator;
    
    [SerializeField] private Transform _normalCameraTarget;

    [Header("Настройки Физики Сети")]
    [SerializeField] float pushPower = 7f; 
    [SerializeField] float speed = 8f;

    [Header("Настройки Сетевого Рывка (Dash)")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.28f;
    
    [Networked] private Vector3 _lastCheckpointPosition { get; set; }
    [Networked] private TickTimer _dashActiveTimer { get; set; } 
    [Networked] private Vector3 _dashDirection { get; set; }
    [Networked] private NetworkBool _hasDashedInAir { get; set; } 

    [Header("Сетевой статус финиша")]
    [Networked] public NetworkBool IsFinished { get; set; }
    [Networked] public int FinishPlace { get; set; }

    [Header("Для анимации")]
    [Networked] private float _netSpeed { get; set; }
    [Networked] private float _netMotionSpeed { get; set; }
    [Networked] private NetworkBool _netGrounded { get; set; }
    [Networked] private NetworkBool _netJumpTrigger { get; set; }
    [Networked] private NetworkBool _netDashTrigger { get; set; }
    
    private bool _isSpawnReady = false; // Предохранитель для первого кадра
    private Vector3 _platformMovement = Vector3.zero;
    private Vector3 _externalVelocityThisTick = Vector3.zero;
    private float _baseMaxSpeedInInspector;

    public override void Spawned()
    {
        _networkController = GetComponent<NetworkCharacterController>();
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

        if (_networkController != null)
        {
            // Шаг 1. Запоминаем ту максимальную скорость, которую вы выставили в инспекторе (например 6 или 8)
            _baseMaxSpeedInInspector = _networkController.maxSpeed;
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
        if (!_isSpawnReady || _networkController == null) return;

        if (IsFinished)
        {
            return; 
        }

        if (GetInput(out NetworkInputData data))
        {
            if (_networkController.Grounded)
            {
                _hasDashedInAir = false;
            }

            if (data.JumpPressed && _networkController.Grounded && _dashActiveTimer.ExpiredOrNotRunning(Runner)) 
            {
                _networkController.Jump();
            }

            Vector3 finalMoveVelocity = Vector3.zero;
            float currentMoveSpeed = 0f;

            // --- 1. ПРОВЕРКА СОСТОЯНИЯ АКТИВНОГО РЫВКА ---
            if (!_dashActiveTimer.ExpiredOrNotRunning(Runner))
            {
                // МАГИЯ УВЕЛИЧЕНИЯ ЛИМИТA: Пока корова летит в рывке, мы разжимаем тиски NCC 
                // и выставляем максимальную скорость равной dashSpeed (20)!
                _networkController.maxSpeed = dashSpeed;

                // Направляем вектор полета строго вперед
                finalMoveVelocity = _dashDirection * dashSpeed;
                currentMoveSpeed = dashSpeed;
            }
            else
            {
                // --- КРИТИЧЕСКИЙ ШАГ: РЫВОК ЗАВЕРШЕН, ВОЗВРАЩАЕМ ЛИМИТ СКОРОСТИ НАЗАД ---
                // Как только таймер иссяк — мгновенно возвращаем NCC его родную скорость бега (например 6)
                _networkController.maxSpeed = _baseMaxSpeedInInspector;

                // Обычный расчет WASD бега
                Vector3 inputDirection = new Vector3(data.MoveDirection.x, 0.0f, data.MoveDirection.y).normalized;

                if (data.MoveDirection != Vector2.zero)
                {
                    Quaternion cameraYRotation = Quaternion.Euler(0f, data.CameraRotationY, 0f);
                    Vector3 camForward = cameraYRotation * Vector3.forward;
                    Vector3 camRight = cameraYRotation * Vector3.right;

                    Vector3 targetDirection = (camForward * inputDirection.z + camRight * inputDirection.x).normalized;
                    transform.forward = Vector3.Slerp(transform.forward, targetDirection, Runner.DeltaTime * 15f);

                    finalMoveVelocity = targetDirection * speed;
                    currentMoveSpeed = speed;
                }

                // --- 2. АКТИВАЦИЯ РЫВКА В ВОЗДУХЕ ---
                if (data.DashPressed && !_networkController.Grounded && !_hasDashedInAir)
                {
                    _hasDashedInAir = true;

                    // Включаем таймер полета рыбкой на 0.28 секунды
                    _dashActiveTimer = TickTimer.CreateFromSeconds(Runner, dashDuration);

                    // Фиксируем направление броска
                    _dashDirection = (data.MoveDirection != Vector2.zero) ? finalMoveVelocity.normalized : transform.forward;

                    // Мгновенно расширяем лимит скорости до взрывного значения
                    _networkController.maxSpeed = dashSpeed;
                    finalMoveVelocity = _dashDirection * dashSpeed;
                    currentMoveSpeed = dashSpeed;

                    _netDashTrigger = true;
                    
                    Debug.Log($"[Рывок 2.1] Лимит MaxSpeed расширен до {dashSpeed}! Корова ушла в полет.");
                }
            }

            _networkController.Move(finalMoveVelocity);

            _netSpeed = currentMoveSpeed;
            _netMotionSpeed = (data.MoveDirection != Vector2.zero) ? 1f : 0f;
            _netGrounded = _networkController.Grounded;

            if (data.JumpPressed) _netJumpTrigger = true;
        }
    }

    public override void Render()
    {
        if (_animator != null)
        {
            _animator.SetFloat("Speed", _netSpeed);
            _animator.SetFloat("MotionSpeed", _netMotionSpeed);
            _animator.SetBool("Grounded", _netGrounded);
            
            if (_netJumpTrigger)
            {
                _animator.SetBool("Jump", true);
                if (HasInputAuthority || Runner.IsServer) _netJumpTrigger = false;
            }
            else
            {
                _animator.SetBool("Jump", false);
            }

            if (_netDashTrigger)
            {
                _animator.SetTrigger("Dive"); 
                
                if (HasInputAuthority || Runner.IsServer) _netDashTrigger = false;
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
        // 1. Принудительно выключаем рэгдолл
        var ragdoll = GetComponent<PlayerRagdoll>();
        if (ragdoll != null)
        {
            ragdoll.ToggleRagdoll(false); 
        }

        yield return new WaitForFixedUpdate();

        // 2. Обнуляем физические скорости костей
        var allRigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRigidbodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. МАГИЯ ТЕЛЕПОРТАЦИИ ДЛЯ NETWORK CHARACTER CONTROLLER:
        // Мы вызываем специальный метод контроллера, который мгновенно сбрасывает скорости 
        // симуляции и переносит корову на чекпоинт на сервере и у всех клиентов одновременно!
        if (_networkController != null)
        {
            _networkController.Teleport(_lastCheckpointPosition);
        }
        else
        {
            // На случай, если компонент потерялся
            transform.position = _lastCheckpointPosition;
        }

        _platformMovement = Vector3.zero;

        yield return new WaitForFixedUpdate();

        Debug.Log($"[Сеть] Физический сетевой респавн завершен успешно! Точка: {_lastCheckpointPosition}");
    }

    public void ApplyNetworkKnockback(Vector3 direction, float force)
    {
        if (_networkController != null && _isSpawnReady)
        {
            _networkController.Velocity += direction * force;
            
            Debug.Log($"[Импульс] К сетевой скорости прибавлен вектор: {direction * force}");
        }
    }

    public void SetNetworkPlatformMovement(Vector3 externalVelocity)
    {
        _externalVelocityThisTick = externalVelocity;
    }

    public void ApplyNetworkTrampolineBounce(float force)
    {
        // Менять [Networked] параметры во Fusion разрешено только владельцу ввода или серверу
        if (HasInputAuthority || Runner.IsServer)
        {
            // Мы напрямую перезаписываем вертикальную скорость! 
            // Вместо плавного прибавления, мы даем резкий мощный пинок строго вверх
            //_verticalVelocity = force;

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