using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion; // Подключаем Fusion для проверки сетевых прав

namespace NonameGame
{
    public class NetworkBumper : MonoBehaviour
    {
        [Header("Настройки бампера")]
        [Tooltip("Сила импульса, с которой столб отбрасывает корову")]
        [SerializeField] private float bounceForce; 
        
        [Tooltip("Время перезарядки бампера для конкретного игрока (в секундах)")]
        [SerializeField] private float cooldownTime;   

        // Черный список, который защищает от множественных ударов за одну миллисекунду
        private HashSet<Rigidbody> _activeRigidbodies = new HashSet<Rigidbody>();

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Rigidbody playerRb = other.GetComponent<Rigidbody>();
                
                if (playerRb != null)
                {
                    if (!_activeRigidbodies.Contains(playerRb))
                    {
                        _activeRigidbodies.Add(playerRb);

                        NetworkObject netObj = playerRb.GetComponent<NetworkObject>();
                        NetworkObject netObj2 = playerRb.GetComponent<NetworkObject>();
                        if (netObj != null)
                        {
                            // Локальное предсказание импульса (Client-Side Prediction):
                            // Силу удара применяет и Хост(Сервер), и Клиент, который управляет этой коровой!
                            // Это дает мгновенный отскок без задержек сети и без дерганий.
                            if (netObj.HasInputAuthority || netObj.Runner.IsServer)
                            {
                                // Вычисляем чистый вектор направления от центра столба к игроку
                                Vector3 bounceDir = (other.transform.position - transform.position);
                                bounceDir.y = 0f; // Игнорируем высоту для честного горизонтального вектора
                                bounceDir = bounceDir.normalized;
                                
                                bounceDir.y = 0.4f; 

                                // Сбрасываем прошлую скорость падения, чтобы импульс подброса сработал
                                playerRb.linearVelocity = Vector3.zero;

                                playerRb.AddForce(bounceDir.normalized * bounceForce, ForceMode.Impulse);
                                
                                Debug.Log($"[Физический Бампер] К Rigidbody игрока приложен импульс силы: {bounceForce}");
                            }
                        }

                        // Автоматически удаляем игрока из блокировки по независимому таймеру
                        StartCoroutine(ReleasePlayerRoutine(playerRb, cooldownTime));
                    }
                }
            }
        }

        private IEnumerator ReleasePlayerRoutine(Rigidbody rb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_activeRigidbodies.Contains(rb))
            {
                _activeRigidbodies.Remove(rb);
            }
        }
    }
}
