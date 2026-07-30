using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    void Start()
    {
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    // Используем FixedUpdate, так как DOTween обновляется в FixedUpdate
    void FixedUpdate()
    {
        // Фиксируем состояние в конце физического кадра
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<ThirdPersonController>();
            if (playerController != null)
            {
                // 1. Вычисляем изменение вращения платформы за физический кадр
                Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(_lastRotation);
                
                // 2. Считаем вектор от центра платформы до игрока
                Vector3 playerOffset = other.transform.position - transform.position;
                
                // 3. Узнаем, где игрок должен оказаться из-за кручения платформы
                Vector3 newPlayerPosition = transform.position + (rotationDelta * playerOffset);
                
                // 4. Получаем чистый вектор сдвига от вращения
                Vector3 movement = newPlayerPosition - other.transform.position;

                // 5. Добавляем линейное движение (на случай, если платформа еще и едет)
                movement += (transform.position - _lastPosition);

                // Игнорируем изменения по вертикали, чтобы игрока не прижимало к полу
                movement.y = 0; 

                // 6. Передаем готовое смещение в скрипт персонажа
                playerController.SetPlatformMovement(movement);

                // 7. Поворачиваем самого игрока вместе с платформой (по желанию)
                other.transform.rotation = rotationDelta * other.transform.rotation;
            }
        }
    }
}
