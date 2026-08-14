using UnityEngine;
using System.Collections;
using UnityEngine.Splines; // ОБЯЗАТЕЛЬНО: подключаем пространство имен сплайнов Unity

public class TubeTraveler : MonoBehaviour
{

    private float _speed;

    private SplineContainer _splineContainer;
    private Transform _hipsTransform;
    private float _progress;
    
    public void SetupPath(SplineContainer splineContainer, PlayerRagdoll ragdoll, float speed) 
    {
        _splineContainer = splineContainer;
        _speed = speed;
        _hipsTransform = ragdoll.HipsTransform;

        StartCoroutine(FlyThroughTube());
    }

    private IEnumerator FlyThroughTube()
    {
        // Получаем общую длину сплайна в метрах
        var splineLength = _splineContainer.CalculateLength();

        while (_progress < 1f)
        {
            // Увеличиваем прогресс движения в зависимости от заданной скорости
            _progress += (_speed / splineLength) * Time.fixedDeltaTime;
            _progress = Mathf.Clamp01(_progress);

            // узнаем точную мировую позицию на сплайне по проценту прогресса
            Vector3 targetPos = _splineContainer.EvaluatePosition(_progress);

            // Двигаем корень игрока по этой точке
            transform.position = targetPos;

            // Подтягиваем кость таза рэгдолла в центр
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
        Debug.Log("Игрок вышел из трубы!");

        Vector3 ejectDirection = _splineContainer.EvaluateTangent(5f);        
        ejectDirection.y = 0.4f;

        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            if (!rb.isKinematic)
            {
                Debug.Log("Выстрел из трубы!");
                rb.AddForce(ejectDirection.normalized, ForceMode.Impulse);
            }
        }

        Destroy(this);
    }
}
