using UnityEngine;
using System.Collections;
using UnityEngine.Splines; // ОБЯЗАТЕЛЬНО: подключаем пространство имен сплайнов Unity

public class TubeTraveler : MonoBehaviour
{
    private SplineContainer _splineContainer;
    private float _speed = 15f; // Скорость полета внутри трубы
    private Transform _hipsTransform;
    private float _progress = 0f; // Прогресс движения от 0 (вход) до 1 (выход)

    public void SetupPath(Transform splineTransform)
    {
        // Получаем компонент SplineContainer, который виден у вас на скриншоте
        _splineContainer = splineTransform.GetComponent<SplineContainer>();
        
        if (_splineContainer == null)
        {
            Debug.LogError("На объекте TubeSpline отсутствует компонент SplineContainer!");
            Destroy(this);
            return;
        }

        // Находим кость таза рэгдолла
        _hipsTransform = transform.Find("mixamorig:Hips") ?? GetComponentInChildren<Rigidbody>().transform;

        StartCoroutine(FlyThroughTube());
    }

    private IEnumerator FlyThroughTube()
    {
        // Получаем общую длину сплайна в метрах
        float splineLength = _splineContainer.CalculateLength();

        while (_progress < 1f)
        {
            // Увеличиваем прогресс движения в зависимости от заданной скорости
            _progress += (_speed / splineLength) * Time.fixedDeltaTime;
            _progress = Mathf.Clamp01(_progress);

            // Магия Unity Splines: узнаем точную мировую позицию на сплайне по проценту прогресса
            Vector3 targetPos = _splineContainer.EvaluatePosition(_progress);

            // Двигаем корень игрока по этой точке
            transform.position = targetPos;

            // Подтягиваем кость таза рэгдолла в центр, чтобы корова не застревала в стенках
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
        // ВЫПЛЁВЫВАНИЕ: Получаем направление финальной точки сплайна (куда смотрит выход трубы)
        Vector3 ejectDirection = (Vector3)_splineContainer.EvaluateTangent(1f);
        ejectDirection.y = 0.4f; // Слегка подбрасываем вверх для красивой дуги полета

        Rigidbody[] allRbs = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRbs)
        {
            if (!rb.isKinematic)
            {
                // Выстреливаем рэгдолл вперед
                rb.AddForce(ejectDirection.normalized, ForceMode.Impulse);
            }
        }

        // Удаляем скрипт полета. Через 3 секунды корова встанет на ноги сама
        Destroy(this);
    }
}
