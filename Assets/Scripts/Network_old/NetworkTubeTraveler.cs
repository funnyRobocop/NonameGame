using UnityEngine;
using System.Collections;
using UnityEngine.Splines;
using Fusion;

public class NetworkTubeTraveler : MonoBehaviour
{
    private SplineContainer _splineContainer;
    private float _speed; 
    private Transform _hipsTransform;
    private float _progress = 0f;
    private float _splineLength;
    private bool _isFlying = false;

    public void SetupPath(SplineContainer splineContainer, NetworkPlayerRagdoll ragdoll, float speed)
    {
        _splineContainer = splineContainer;
        _speed = speed;
        _hipsTransform = ragdoll.HipsTransform;
        
        if (_splineContainer != null)
        {
            _splineLength = _splineContainer.CalculateLength();
            _progress = 0f;
            _isFlying = true;
        }
    }

    void FixedUpdate()
    {
        if (!_isFlying || _splineContainer == null) return;

        _progress += (_speed / _splineLength) * Time.fixedDeltaTime;
        _progress = Mathf.Clamp01(_progress);

        Vector3 targetPos = (Vector3)_splineContainer.EvaluatePosition(_progress);

        //transform.position = targetPos;
        _hipsTransform.position = targetPos;

        // Если долетели до конца сплайна (выход из трубы)
        if (_progress >= 1f)
        {
            _isFlying = false;
            ExitTube();
        }
    }

    private void ExitTube()
    {
        // ВЫПЛЁВЫВАНИЕ: Вычисляем вектор выхода из трубы
        Vector3 ejectDirection = (Vector3)_splineContainer.EvaluateTangent(1f);
        ejectDirection.y = 0.4f; // Подбрасываем вверх по дуге

        float ejectForce = 5f;

        // Прикладываем физический импульс локально. Физика PhysX подхватит кости,
        // а NetworkTransform плавно покажет этот вылет всем остальным в лобби
        Rigidbody[] allRbs = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRbs)
        {
            if (!rb.isKinematic)
            {
                rb.AddForce(ejectDirection.normalized * ejectForce, ForceMode.Impulse);
            }
        }

        Debug.Log("[Труба] Корова успешно вылетела из трубы!");
        Destroy(this);
    }
}
