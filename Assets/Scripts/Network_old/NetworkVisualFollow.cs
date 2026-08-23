using PhysicsCharacterController;
using UnityEngine;

public class NetworkVisualFollow : MonoBehaviour
{
    [Header("Настройки сглаживания")]
    [Tooltip("Скорость плавного следования за физическим телом. Чем выше, тем отзывчивее графика.")]
    [SerializeField] private float positionLerpSpeed = 25f;
    [Tooltip("Скорость плавного разворота графической модели.")]
    [SerializeField] private float rotationLerpSpeed = 15f;

    [Header("Фильтрация шума пинга (Threshold)")]
    [Tooltip("Минимальная дистанция сдвига (в метрах), на которую графика вообще не будет реагировать. Убирает дрожание при низком пинге!")]
    [SerializeField] private float positionThreshold = 0.005f; // 5 миллиметров
    [SerializeField] public Transform _normalCameraTarget;

    private Transform _targetPhysicsTransform;
    private Animator _animator;
    private Rigidbody _targetRigidbody;
    private CharacterManager _targetCharacterManager;

    // Метод вызывается кодом при спавне коровы, чтобы передать графике цель для следования
    public void InitializeFollowTarget(Transform physicsTarget, CharacterManager manager, Rigidbody rb)
    {
        _targetPhysicsTransform = physicsTarget;
        _targetCharacterManager = manager;
        _targetRigidbody = rb;
        
        // Мгновенно перемещаем графику в точку старта, чтобы не было летящего шлейфа из нулевых координат
        transform.position = physicsTarget.position;
        transform.rotation = physicsTarget.rotation;

        _animator = GetComponentInChildren<Animator>();
    }

    // Используем стандартный LateUpdate от Unity! 
    // Он срабатывает строго ПОСЛЕ того, как все сетевые тики Fusion и физика PhysX 
    // закончили двигать корень, и идеально совпадает с рендерингом камеры Cinemachine!
    private void LateUpdate()
    {
        if (_targetPhysicsTransform == null) return;

        // --- 1. ПЛАНЫЙ СЛЕДУЮЩИЙ LERP ПОЗИЦИИ С ФИЛЬТРАЦИЕЙ ШУМА ---
        float distanceToTarget = Vector3.Distance(transform.position, _targetPhysicsTransform.position);

        // Если физическое тело сдвинулось дальше, чем наш порог чувствительности (Threshold)
        if (distanceToTarget > positionThreshold)
        {
            // Если корова улетела слишком далеко (например, респаун на чекпоинте воды), 
            // мгновенно телепортируем графику, чтобы она не летела через всю карту со скоростью света
            if (distanceToTarget > 3f)
            {
                transform.position = _targetPhysicsTransform.position;
            }
            else
            {
                // Плавно лерпим позицию графического контейнера на частоте вашего монитора!
                transform.position = Vector3.Lerp(transform.position, _targetPhysicsTransform.position, Time.deltaTime * positionLerpSpeed);
            }
        }

        // --- 2. ПЛАВНЫЙ РАЗВОРOT МОДЕЛИ ---
        // Графика послушно заимствует разворот физического корня, сглаживая углы
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetPhysicsTransform.rotation, Time.deltaTime * rotationLerpSpeed);

        // --- 3. СИНХРОНИЗАЦИЯ АНИМАТОРА НАПРЯМУЮ С RIGIDBODY ---
        if (_animator != null && _targetRigidbody != null && _targetCharacterManager != null)
        {
            // Считываем скорость прямо из сетевого Rigidbody
            Vector3 horizontalVel = new Vector3(_targetRigidbody.linearVelocity.x, 0f, _targetRigidbody.linearVelocity.z);
            
            // Передаем сглаженную скорость в аниматор локально
            _animator.SetFloat("Speed", horizontalVel.magnitude);
            _animator.SetBool("Grounded", _targetCharacterManager.GetGrounded());

            if (_targetCharacterManager.netDashAnimationFlag)
            {
                // Включаем триггер анимации прыжка рыбкой / броска вперед (например "Dive" или "Dash")
                _animator.SetTrigger("Dive"); 
                
                // Сразу же сбрасываем флаг, чтобы анимация не зациклилась
                _targetCharacterManager.netDashAnimationFlag = false;
            }
        }
    }
}
