using UnityEngine;


public class NetworkBumper : MonoBehaviour
    {
        [Header("Настройки отскока")]
        [SerializeField] private float bounceForce = 15f; // Сила отталкивания от столба

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var networkPlayer = other.GetComponent<NetworkPlayerController>();
                
                if (networkPlayer != null)
                {
                    // Вычисляем горизонтальное направление от центра столба к игроку
                    Vector3 bounceDir = (other.transform.position - transform.position);
                    bounceDir.y = 0f; // Обнуляем высоту, чтобы сначала получить чистый вектор вбок
                    bounceDir = bounceDir.normalized;

                    // Добавляем небольшой подброс вверх (как в Fall Guys), чтобы игрок эпично отлетал по дуге
                    bounceDir.y = 0.4f; 

                    // Передаем сетевой импульс игроку
                    networkPlayer.ApplyNetworkKnockback(bounceDir.normalized, bounceForce);
                    
                    Debug.Log($"[Батут-Столб] Игрок получил импульс отскока: {bounceDir * bounceForce}");
                }
            }
        }
    }
