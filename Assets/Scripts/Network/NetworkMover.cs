using UnityEngine;
using Fusion;

public class NetworkMover : NetworkBehaviour
{
    [Header("Настройки перемещения")]
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 0f, 5f); // На сколько метров сдвигаться
    [SerializeField] private float speed = 2f;                             // Скорость движения
    [SerializeField] private float timeOffset = 0f;                        // Задержка фазы времени

    private Vector3 _startLocalPos;

    public override void Spawned()
    {
        _startLocalPos = transform.localPosition;
    }

    public override void FixedUpdateNetwork()
    {
        float syncedTime = Runner.SimulationTime + timeOffset;

        // Используем Mathf.PingPong для циклического движения туда-обратно от 0 до 1
        float pingPong = Mathf.PingPong(syncedTime * speed, 1f);
        
        // Плавное сглаживание углов (Ease In Out), чтобы объект не менял направление слишком резко
        float smoothPingPong = Mathf.SmoothStep(0f, 1f, pingPong);

        // Вычисляем новую локальную позицию
        transform.localPosition = _startLocalPos + (moveOffset * smoothPingPong);
    }
}
