using UnityEngine;
using System.Collections;
using UnityEngine.Splines; // ОБЯЗАТЕЛЬНО: подключаем пространство имен сплайнов Unity

public class TubeTraveler : MonoBehaviour
{
    private SplineContainer _splineContainer;
    private float _speed = 5f; // Скорость полета внутри трубы
    private Transform _hipsTransform;
    private float _progress = 0f; // Прогресс движения от 0 (вход) до 1 (выход)
    private  Rigidbody _hipsRigidbody;
    
    public void SetupPath(SplineContainer splineContainer, PlayerRagdoll ragdoll) 
    {
        _splineContainer = splineContainer;

        _hipsTransform = ragdoll.HipsTransform;
        _hipsRigidbody = _hipsTransform.GetComponent<Rigidbody>();

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
                // Легкий импульс вперед для плавного движения
                //_hipsRigidbody.AddRelativeForce(Vector3.forward * 0.5f, ForceMode.VelocityChange);
            }

            yield return new WaitForFixedUpdate();
        }

        ExitTube();
    }

    private void ExitTube()
    {
        Debug.Log("Игрок вышел из трубы!");

        //Получаем направление финальной точки сплайна
        Vector3 ejectDirection = _splineContainer.EvaluateTangent(5f); 
        // Слегка подбрасываем вверх
        ejectDirection.y = 0.4f;
        var allRbs = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRbs)
        {
            if (!rb.isKinematic)
            {
                // Выстреливаем рэгдолл вперед
                rb.AddForce(ejectDirection.normalized, ForceMode.Impulse);
            }
        }

        Destroy(this);
    }
}
