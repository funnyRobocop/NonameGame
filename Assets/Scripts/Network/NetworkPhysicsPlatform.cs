using UnityEngine;
using System.Collections.Generic;
using Fusion; // Подключаем Fusion

namespace NonameGame
{
    // Наследуемся от NetworkBehaviour, чтобы платформа работала в едином сетевом тике с игроком!
    public class NetworkPhysicsPlatform : NetworkBehaviour
    {
        // Список физических объектов (коров), которые СЕЙЧАС стоят на платформе
        private List<Rigidbody> _activePlayers = new List<Rigidbody>();

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;

        public override void Spawned()
        {
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null && !_activePlayers.Contains(rb))
                {
                    _activePlayers.Add(rb);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null && _activePlayers.Contains(rb))
                {
                    _activePlayers.Remove(rb);
                }
            }
        }

        // ГЛАВНЫЙ СЕТЕВОЙ ЦИКЛ СИНХРОНИЗАЦИИ
        public override void FixedUpdateNetwork()
        {
            _activePlayers.RemoveAll(item => item == null);

            if (_activePlayers.Count == 0)
            {
                // Запоминаем позицию текущего тика для следующего кадра
                _lastPosition = transform.position;
                _lastRotation = transform.rotation;
                return;
            }

            // ВАЖНО ДЛЯ FUSION 2.1+: Физическое смещение игроков на платформе 
            // должен рассчитывать СТРОГО только Сервер (Хост)!
            // А сетевой Network Transform на игроках сам плавно сгладит этот сдвиг для Клиента.
            if (Runner.IsServer)
            {
                // 1. Вычисляем дельту (микро-сдвиг) позиции платформы за этот сетевой тик
                Vector3 positionDelta = transform.position - _lastPosition;

                // 2. Вычисляем дельту разворота платформы за этот сетевой тик
                Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(_lastRotation);

                foreach (Rigidbody playerRb in _activePlayers)
                {
                    // --- ШАГ А. ЛИНЕЙНОЕ СМЕЩЕНИЕ ---
                    // Просто переносим позицию Rigidbody вслед за едущей платформой
                    playerRb.position += positionDelta;

                    // --- ШАГ Б. УГЛОВОЕ ВРАЩЕНИЕ (То, чего не хватало!) ---
                    // Вычисляем вектор расстояния от центра крутящегося диска до коровы
                    Vector3 distanceVector = playerRb.position - transform.position;
                    
                    // Вращаем саму точку нахождения игрока по орбите диска
                    Vector3 newPosition = transform.position + (rotationDelta * distanceVector);
                    playerRb.position = newPosition;

                    // Намертво разворачиваем саму модельку коровы лицом по направлению вращения платформы!
                    playerRb.rotation = rotationDelta * playerRb.rotation;

                    // Обновляем целевой угол поворота внутри скрипта Nappin, 
                    // чтобы при нажатии WASD корова не дергалась обратно в старый угол взгляда камеры!
                    var characterManager = playerRb.GetComponent<PhysicsPlayerController>();
                    if (characterManager != null)
                    {
                        // Прибавляем дельту угла платформы к сетевому целевому углу персонажа
                        characterManager.targetAngle += rotationDelta.eulerAngles.y;
                    }
                }
            }

            // Запоминаем текущее состояние для следующего тика симуляции Fusion
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
        }
    }
}
