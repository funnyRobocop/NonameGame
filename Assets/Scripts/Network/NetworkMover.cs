using UnityEngine;
using Fusion;

public class NetworkMover : NetworkBehaviour
{
    [Header("Настройки перемещения")]
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 0f, 5f); // Смещение относительно старта
    [SerializeField] private float speed = 2f;                             // Скорость движения
    [SerializeField] private float timeOffset = 0f;                        // Смещение фазы времени

    private Vector3 _startWorldPos;
    private Rigidbody _rigidbody;

    public override void Spawned()
    {
        _startWorldPos = transform.position;        
        _rigidbody = gameObject.GetComponent<Rigidbody>();

        if (_rigidbody == null)
        {
            Debug.LogError($"[Сбой] На объекте {gameObject.name} отсутствует Rigidbody!");
        }
    }

    public override void Render()
    {
        // Получаем синхронизированное время сервера
        float syncedTime = Runner.SimulationTime + timeOffset;

        // Циклическое движение туда-обратно в диапазоне от 0 до 1
        float pingPong = Mathf.PingPong(syncedTime * speed, 1f);
        
        // Математическое сглаживание в крайних точках (плавный разгон и торможение)
        float smoothPingPong = Mathf.SmoothStep(0f, 1f, pingPong);

        // Вычисляем целевую мировую координату для этого сетевого тика
        Vector3 targetWorldPosition = _startWorldPos + (moveOffset * smoothPingPong);
        
        //_rigidbody.MovePosition(targetWorldPosition);
        transform.position = targetWorldPosition; // Обновляем позицию объекта для визуализации
    }
}
