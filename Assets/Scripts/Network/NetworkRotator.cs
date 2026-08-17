using UnityEngine;
using Fusion;

public class NetworkRotator : NetworkBehaviour
{
    public enum RotationMode { Constant, Pendulum }

    [Header("Тип движения")]
    [SerializeField] private RotationMode mode = RotationMode.Constant;

    [Header("Настройки вращения")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up; 
    [SerializeField] private float speed = 50f;                 
    [SerializeField] private float maxAngle = 90f;              
    [SerializeField] private float timeOffset = 0f;
    
    private Rigidbody _rigidbody;
    private Quaternion _startRotation;

    public override void Spawned()
    {
        _startRotation = transform.localRotation;
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        
        if (_rigidbody == null)
        {
            Debug.LogError($"[Сбой] На объекте {gameObject.name} отсутствует Rigidbody! Сетевое вращение будет дергаться.");
        }
    }

    public override void FixedUpdateNetwork()
    {
        float syncedTime = Runner.SimulationTime + timeOffset;
        Quaternion targetRotation = _startRotation;

        if (mode == RotationMode.Constant)
        {
            float currentAngle = syncedTime * speed;
            targetRotation = _startRotation * Quaternion.AngleAxis(currentAngle, rotationAxis);
        }
        else if (mode == RotationMode.Pendulum)
        {
            float currentAngle = Mathf.Sin(syncedTime * (speed * 0.1f)) * maxAngle;
            targetRotation = _startRotation * Quaternion.AngleAxis(currentAngle, rotationAxis);
        }
        
        _rigidbody.MoveRotation(targetRotation);
    }
}
