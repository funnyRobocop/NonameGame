using UnityEngine;
using Fusion;

public class NetworkGateTrap : NetworkBehaviour
    {
        [Header("Настройки вращения")]
        [SerializeField] private float targetAngle = 90f;   // Угол подъема платформы (в градусах)
        [SerializeField] private Vector3 rotationAxis = Vector3.right; // Ось X (красная стрелка)

        [Header("Тайминги фаз (в секундах)")]
        [SerializeField] private float cycleDuration = 4f;   // Полное время одного цикла (интервал)
        [SerializeField] private float riseDuration = 0.15f; // Скорость резкого подъема (очень быстро)
        [SerializeField] private float stayDuration = 0.8f;  // Сколько секунд платформа стоит вертикально
        [SerializeField] private float fallDuration = 0.8f;  // Время плавного опускания назад

        [Header("Смещение фазы (Очередь)")]
        [SerializeField] private float timeOffset = 0f;     // Позволяет делать "волну" (например: 0, 0.5, 1.0)

        private Quaternion _startRotation;

        public override void Spawned()
        {
            // Запоминаем исходный наклон платформы при старте сети
            _startRotation = transform.localRotation;
        }

        public override void FixedUpdateNetwork()
        {
            // Синхронизированное серверное время с учетом индивидуального смещения
            float syncedTime = Runner.SimulationTime + timeOffset;

            // Вычисляем, какая секунда идет внутри текущего интервала (цикла)
            float timeInCycle = syncedTime % cycleDuration;

            float progress = 0f;

            // 1. ФАЗА: Резкий подъем вверх
            if (timeInCycle < riseDuration)
            {
                float t = timeInCycle / riseDuration;
                progress = Mathf.SmoothStep(0f, 1f, t); // Взрывной подъем
            }
            // 2. ФАЗА: Удержание в верхнем положении (90 градусов)
            else if (timeInCycle < riseDuration + stayDuration)
            {
                progress = 1f;
            }
            // 3. ФАЗА: Плавное опускание обратно под землю / в горизонт
            else if (timeInCycle < riseDuration + stayDuration + fallDuration)
            {
                float t = (timeInCycle - (riseDuration + stayDuration)) / fallDuration;
                progress = Mathf.SmoothStep(1f, 0f, t); // Мягкий возврат
            }
            // 4. ФАЗА: Платформа просто лежит и ждет игроков (progress = 0)
            else
            {
                progress = 0f;
            }

            // Вычисляем текущий поворот: стартовый угол + (целевой угол * прогресс фазы)
            float currentAngle = targetAngle * progress;
            transform.localRotation = _startRotation * Quaternion.AngleAxis(currentAngle, rotationAxis);
        }
    }
