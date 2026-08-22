using UnityEngine;
using Fusion;

public class NetworkGateTrap : NetworkBehaviour
{
    [Header("Настройки вращения")]
    [SerializeField] private float targetAngle = 90f;   
    [SerializeField] private Vector3 rotationAxis = Vector3.right; 

    [Header("Тайминги фаз (в секундах)")]
    [SerializeField] private float cycleDuration = 4f;   
    [SerializeField] private float riseDuration = 0.15f; 
    [SerializeField] private float stayDuration = 0.8f;  
    [SerializeField] private float fallDuration = 0.8f;  
    [SerializeField] private float timeOffset = 0f;     

    private Quaternion _startRotation;
    //private Rigidbody _rigidbody;

    public override void Spawned()
    {
        _startRotation = transform.localRotation;
        /*_rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody == null)
        {
            Debug.LogWarning($"[Внимание] На платформе {gameObject.name} не найден Rigidbody!");
        }*/
    }

    public override void Render()
    {
        float syncedTime = Runner.SimulationTime + timeOffset;
        float timeInCycle = syncedTime % cycleDuration;
        float progress = 0f;

        if (timeInCycle < riseDuration)
        {
            float t = timeInCycle / riseDuration;
            progress = Mathf.SmoothStep(0f, 1f, t);
        }
        else if (timeInCycle < riseDuration + stayDuration)
        {
            progress = 1f;
        }
        else if (timeInCycle < riseDuration + stayDuration + fallDuration)
        {
            float t = (timeInCycle - (riseDuration + stayDuration)) / fallDuration;
            progress = Mathf.SmoothStep(1f, 0f, t);
        }
        else
        {
            progress = 0f;
        }

        float currentAngle = targetAngle * progress;
        Quaternion targetLocalRotation = _startRotation * Quaternion.AngleAxis(currentAngle, rotationAxis);

        /*if (_rigidbody != null)
        {
            // Для MoveRotation переводим локальный поворот в мировой относительно родителя
            Quaternion targetWorldRotation = transform.parent != null ? transform.parent.rotation * targetLocalRotation : targetLocalRotation;
            _rigidbody.MoveRotation(targetWorldRotation);
        }
        else
        {
            transform.localRotation = targetLocalRotation;
        }*/

        transform.localRotation = targetLocalRotation;
    }
}
