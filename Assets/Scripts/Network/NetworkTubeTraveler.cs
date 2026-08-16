using UnityEngine;
using System.Collections;
using UnityEngine.Splines;
using Fusion;

// Оставляем MonoBehaviour, так как скрипт добавляется временно компонентом
public class NetworkTubeTraveler : MonoBehaviour
{
    private SplineContainer _splineContainer;
    private float _speed;
    private Transform _hipsTransform;
    private float _progress = 0f;
    private NetworkPlayerController _playerController;

    public void SetupPath(SplineContainer splineContainer, NetworkPlayerRagdoll ragdoll, float speed)
    {
        _splineContainer = splineContainer;
        _speed = speed;
        _hipsTransform = ragdoll.HipsTransform;
        _playerController = GetComponent<NetworkPlayerController>();

        StartCoroutine(FlyThroughTube());
    }

    private IEnumerator FlyThroughTube()
    {
        float splineLength = _splineContainer.CalculateLength();

        while (_progress < 1f)
        {
            // Используем Runner.DeltaTime из сетевого контроллера, чтобы скорость полета была одинаковой при любом пинге
            if (_playerController != null && _playerController.Runner != null)
            {
                _progress += (_speed / splineLength) * _playerController.Runner.DeltaTime;
            }
            else
            {
                _progress += (_speed / splineLength) * Time.fixedDeltaTime;
            }
            
            _progress = Mathf.Clamp01(_progress);

            // Вычисляем мировую позицию на сплайне
            Vector3 targetPos = _splineContainer.EvaluatePosition(_progress);

            // Перемещаем корень. NetworkTransform на префабе автоматически перешлет эти координаты другим игрокам
            transform.position = targetPos;

            if (_hipsTransform != null)
            {
                _hipsTransform.position = targetPos;
            }

            yield return new WaitForFixedUpdate();
        }

        ExitTube();
    }

    private void ExitTube()
    {
        // ВЫПЛЁВЫВАНИЕ: Вычисляем вектор выхода из трубы
        /*Vector3 ejectDirection = (Vector3)_splineContainer.EvaluateTangent(1f);
        ejectDirection.y = 0.4f; // Подбрасываем вверх по дуге

        float ejectForce = 30f;

        // Прикладываем физический импульс локально. Физика PhysX подхватит кости,
        // а NetworkTransform плавно покажет этот вылет всем остальным в лобби
        Rigidbody[] allRbs = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRbs)
        {
            if (!rb.isKinematic)
            {
                rb.AddForce(ejectDirection.normalized * ejectForce, ForceMode.Impulse);
            }
        }*/

        Debug.Log("[Труба] Корова успешно вылетела из трубы в мультиплеере!");
        
        // Самоуничтожаем скрипт полета. 
        // Наш умный скрипт PlayerRagdoll, который мы обновили через RPC, сам поднимет корову на ноги, как только она коснется земли!
        Destroy(this);
    }
}
