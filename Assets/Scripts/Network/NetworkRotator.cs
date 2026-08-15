using UnityEngine;
using Fusion;

public class NetworkRotator : NetworkBehaviour
    {
        public enum RotationMode { Constant, Pendulum }

        [Header("Тип движения")]
        [SerializeField] private RotationMode mode = RotationMode.Constant;

        [Header("Настройки вращения")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up; // Ось вращения (Y для вентилятора, X/Z для маятника)
        [SerializeField] private float speed = 50f;                 // Скорость вращения
        [SerializeField] private float maxAngle = 90f;              // Максимальный угол (только для маятника)
        
        [Header("Смещение фазы (Очередь)")]
        [SerializeField] private float timeOffset = 0f;             // Позволяет пускать ловушки не одновременно

        private Quaternion _startRotation;

        public override void Spawned()
        {
            // Запоминаем стартовый поворот объекта при создании в сети
            _startRotation = transform.localRotation;
        }

        public override void FixedUpdateNetwork()
        {
            // Runner.SimulationTime — это точное серверное время в секундах
            float syncedTime = Runner.SimulationTime + timeOffset;

            if (mode == RotationMode.Constant)
            {
                // ЛОГИКА ВЕНТИЛЯТОРА: Бесконечное плавное кручение в одну сторону
                float currentAngle = syncedTime * speed;
                transform.localRotation = _startRotation * Quaternion.AngleAxis(currentAngle, rotationAxis);
            }
            else if (mode == RotationMode.Pendulum)
            {
                // ЛОГИКА КУВАЛДЫ: Качание туда-сюда с использованием синуса
                // Mathf.Sin возвращает значения от -1 до 1, плавно замедляясь в крайних точках
                float currentAngle = Mathf.Sin(syncedTime * (speed * 0.1f)) * maxAngle;
                transform.localRotation = _startRotation * Quaternion.AngleAxis(currentAngle, rotationAxis);
            }
        }
    }
