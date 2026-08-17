using UnityEngine;
using Fusion;

public class NetworkMovingPlatform : NetworkBehaviour
{
    [Header("Настройки вращения")]
    [SerializeField] private float rotationSpeed = 50f; 
    [SerializeField] private bool clockWise = true;      
    [SerializeField] private float compensation = 1.3f; // Коэффициент трения
    [SerializeField] private float lerpSpeed = 10f;     // Скорость сглаживания рывков

    [Header("Центр Вращения")]
    [SerializeField] private Transform rotationCenter; 

    // Локальные переменные для сглаживания вектора между тиками
    private Vector3 _targetPlatformMovement = Vector3.zero;
    private Vector3 _currentPlatformMovement = Vector3.zero;
    private NetworkPlayerController _detectedPlayer;

    public override void Spawned()
    {
        if (rotationCenter == null) rotationCenter = transform.root;
    }

    // 1. Физический триггер только фиксирует присутствие и считает сырой вектор
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            
            if (playerController != null && playerController.HasInputAuthority)
            {
                _detectedPlayer = playerController;

                Transform center = rotationCenter != null ? rotationCenter : transform;
                Vector3 upAxis = center.up;
                
                Vector3 toPlayer = other.transform.position - center.position;
                Vector3 projectedOffset = Vector3.ProjectOnPlane(toPlayer, upAxis);
                float currentRadius = projectedOffset.magnitude;

                if (currentRadius > 0.1f)
                {
                    Vector3 movementDirection = Vector3.Cross(upAxis, projectedOffset.normalized).normalized;
                    if (!clockWise) movementDirection = -movementDirection;

                    float linearVelocity = (rotationSpeed * Mathf.Deg2Rad) * currentRadius;
                    
                    // Считаем целевой вектор шага за один тик
                    _targetPlatformMovement = movementDirection * (linearVelocity * Runner.DeltaTime) /** compensation*/;
                }
            }
        }
    }

    // Когда корова слетает с платформы — обнуляем все переменные
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _targetPlatformMovement = Vector3.zero;
            _currentPlatformMovement = Vector3.zero;
            _detectedPlayer = null;
        }
    }

    // 2. ГЛАВНЫЙ СЕТЕВОЙ ЦИКЛ: Здесь происходит плавный Lerp и передача вектора в игрока
    public override void FixedUpdateNetwork()
    {
        if (_detectedPlayer == null) return;

        // Плавно интерполируем (Лерпим) текущую скорость к целевой, убирая любые микро-рывки
        _currentPlatformMovement = Vector3.Lerp(_currentPlatformMovement, _targetPlatformMovement, Runner.DeltaTime * lerpSpeed);

        if (_currentPlatformMovement.magnitude > 0.001f)
        {
            // Передаем идеально сглаженный вектор в контроллер игрока
            _detectedPlayer.SetNetworkPlatformMovement(_currentPlatformMovement);

            // Плавно вращаем корову по орбите диска строго в такт сетевому тику
            float directionSign = clockWise ? 1f : -1f;
            Quaternion rotDelta = Quaternion.Euler(0, rotationSpeed * directionSign * Runner.DeltaTime, 0);
            //_detectedPlayer.transform.rotation = rotDelta * _detectedPlayer.transform.rotation;
        }
    }
}