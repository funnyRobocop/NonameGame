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
                    // ВАЖНО ДЛЯ МУЛЬТИПЛЕЕРА: Импульс отскока прикладывает ТОЛЬКО тот клиент, 
                    // который физически управляет этой конкретной коровой (или Сервер)
                    if (networkPlayer.HasInputAuthority || networkPlayer.Runner.IsServer)
                    {
                        Vector3 bounceDir = (other.transform.position - transform.position);
                        bounceDir.y = 0f; 
                        bounceDir = bounceDir.normalized;
                        bounceDir.y = 0.4f; // Подброс вверх

                        // Передаем импульс
                        networkPlayer.ApplyNetworkKnockback(bounceDir.normalized, bounceForce);
                        
                        Debug.Log($"[Батут] Локальный расчет отскока выполнен успешно.");
                    }
                }
            }
        }
    }
