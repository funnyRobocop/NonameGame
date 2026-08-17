using UnityEngine;


public class NetworkBumper : MonoBehaviour
    {
        [Header("Настройки отскока")]
        [SerializeField] private float bounceForce = 15f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var networkPlayer = other.GetComponent<NetworkPlayerController>();
                
                if (networkPlayer != null)
                {
                    if (networkPlayer.HasInputAuthority || networkPlayer.Runner.IsServer)
                    {
                        Vector3 bounceDir = (other.transform.position - transform.position);
                        bounceDir.y = 0f; 
                        bounceDir = bounceDir.normalized;
                        bounceDir.y = 0.4f;

                        networkPlayer.ApplyNetworkKnockback(bounceDir.normalized, bounceForce);
                        
                        Debug.Log($"[Батут] Локальный расчет отскока выполнен успешно.");
                    }
                }
            }
        }
    }
