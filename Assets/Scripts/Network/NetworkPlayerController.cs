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
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashCooldown = 1.5f;
    
    [Networked] private Vector3 _lastCheckpointPosition { get; set; }
    [Networked] private TickTimer _dashCooldownTimer { get; set; }

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
            if (data.JumpPressed)
            {
                _networkController.Jump();
            }

            Vector3 inputDirection = new Vector3(data.MoveDirection.x, 0.0f, data.MoveDirection.y).normalized;
            Vector3 moveVelocity = Vector3.zero;
            float currentMoveSpeed = 0f;
            Vector3 dashDirection = transform.forward;

            if (data.MoveDirection != Vector2.zero)
            {
                Quaternion cameraYRotation = Quaternion.Euler(0f, data.CameraRotationY, 0f);
                Vector3 camForward = cameraYRotation * Vector3.forward;
                Vector3 camRight = cameraYRotation * Vector3.right;

                Vector3 targetDirection = (camForward * inputDirection.z + camRight * inputDirection.x).normalized;
                transform.forward = Vector3.Slerp(transform.forward, targetDirection, Runner.DeltaTime * 15f);

                moveVelocity = targetDirection * speed;
                currentMoveSpeed = speed;

                dashDirection = targetDirection;
            }

            /*if (_externalVelocityThisTick != Vector3.zero)
            {
                moveVelocity += _externalVelocityThisTick;
                
                _externalVelocityThisTick = Vector3.zero;
            }*/

            _networkController.Move(moveVelocity);

            if (data.DashPressed && _dashCooldownTimer.ExpiredOrNotRunning(Runner))
            {
                // Запускаем таймер перезарядки на сервере строго по тикам Fusion
                _dashCooldownTimer = TickTimer.CreateFromSeconds(Runner, dashCooldown);

                // Прямой физический импульс! Принудительно инжектируем силу рывка в Velocity контроллера.
                // Это мгновенно перебьет ограничение Max Speed и выстрелит игрока вперед!
                Vector3 dashImpulse = dashDirection * dashForce;
                
                // Сохраняем текущую вертикальную скорость (чтобы рывок в воздухе сохранял гравитацию)
                dashImpulse.y = _networkController.Velocity.y; 
                
                _networkController.Velocity = dashImpulse;

                // Включаем триггер анимации рывка
                _netDashTrigger = true;
                
                Debug.Log($"[Сеть] Корова совершила рывок вперед с силой {dashForce}!");
            }

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