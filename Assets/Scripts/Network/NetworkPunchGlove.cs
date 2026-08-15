using UnityEngine;
using Fusion;

public class NetworkPunchGlove : NetworkBehaviour
    {
        [Header("Настройки удара")]
        [SerializeField] private float punchDistance = -3f; // Расстояние вылета по оси Z
        [SerializeField] private float cycleDuration = 3f;   // Полное время цикла ловушки (например, удар каждые 3 секунды)
        [SerializeField] private float punchSpeed = 0.15f;  // Скорость вылета вперед (очень резко)
        [SerializeField] private float stayDuration = 0.5f; // Сколько секунд перчатка удерживается вытянутой
        [SerializeField] private float returnDuration = 1f; // Время плавного возврата назад
        [SerializeField] private float timeOffset = 0f;     // Смещение времени (очередь для разных перчаток)

        private Vector3 _startLocalPos;

        public override void Spawned()
        {
            _startLocalPos = transform.localPosition;
        }

        public override void FixedUpdateNetwork()
        {
            float syncedTime = Runner.SimulationTime + timeOffset;

            // Узнаем, какая секунда идет внутри текущего цикла
            float timeInCycle = syncedTime % cycleDuration;

            float progress = 0f;

            // 1. Фаза резкого вылета вперед
            if (timeInCycle < punchSpeed)
            {
                float t = timeInCycle / punchSpeed;
                progress = Mathf.SmoothStep(0f, 1f, t); // Плавный взрывной разгон
            }
            // 2. Фаза удержания перчатки в вытянутом состоянии
            else if (timeInCycle < punchSpeed + stayDuration)
            {
                progress = 1f;
            }
            // 3. Фаза плавного возврата назад в исходную точку
            else if (timeInCycle < punchSpeed + stayDuration + returnDuration)
            {
                float t = (timeInCycle - (punchSpeed + stayDuration)) / returnDuration;
                progress = Mathf.SmoothStep(1f, 0f, t); // Плавное затухание скорости
            }
            // 4. Остаток времени до конца цикла — перчатка просто отдыхает в базе (progress = 0)
            else
            {
                progress = 0f;
            }

            // Сдвигаем перчатку строго по локальной оси Z на основе высчитанного прогресса фазы
            transform.localPosition = new Vector3(_startLocalPos.x, _startLocalPos.y, _startLocalPos.z + (punchDistance * progress));
        }
    }