using UnityEngine;
using Fusion;

public class NetworkPunchGlove : NetworkBehaviour
{
    [Header("Настройки удара")]
    [SerializeField] private float punchDistance = -3f; 
    [SerializeField] private float cycleDuration = 3f;   
    [SerializeField] private float punchSpeed = 0.15f;  
    [SerializeField] private float stayDuration = 0.5f; 
    [SerializeField] private float returnDuration = 1f; 
    [SerializeField] private float timeOffset = 0f;     

    private Vector3 _startWorldPos; // Для Rigidbody используем мировые координаты
    private Rigidbody _rigidbody;

    public override void Spawned()
    {
        _startWorldPos = transform.position;
        _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody == null)
        {
            Debug.LogWarning($"[Внимание] На перчатке {gameObject.name} не найден Rigidbody!");
        }
    }

    public override void FixedUpdateNetwork()
    {
        float syncedTime = Runner.SimulationTime + timeOffset;
        float timeInCycle = syncedTime % cycleDuration;
        float progress = 0f;

        if (timeInCycle < punchSpeed)
        {
            float t = timeInCycle / punchSpeed;
            progress = Mathf.SmoothStep(0f, 1f, t);
        }
        else if (timeInCycle < punchSpeed + stayDuration)
        {
            progress = 1f;
        }
        else if (timeInCycle < punchSpeed + stayDuration + returnDuration)
        {
            float t = (timeInCycle - (punchSpeed + stayDuration)) / returnDuration;
            progress = Mathf.SmoothStep(1f, 0f, t);
        }
        else
        {
            progress = 0f;
        }

        // Вычисляем целевую мировую позицию перчатки (используем transform.forward для направления вылета)
        Vector3 targetWorldPos = _startWorldPos + (transform.forward * punchDistance * progress);

        if (_rigidbody != null)
        {
            _rigidbody.MovePosition(targetWorldPos);
        }
        else
        {
            transform.position = targetWorldPos;
        }
    }
}