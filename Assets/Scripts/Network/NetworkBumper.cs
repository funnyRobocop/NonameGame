using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetworkBumper : MonoBehaviour
{
    [Header("Настройки бампера")]
    [SerializeField] private float bounceForce = 15f; 
    [SerializeField] private float stunTime = 0.35f;   

    private HashSet<NetworkPlayerController> _activePlayers = new HashSet<NetworkPlayerController>();

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            
            if (playerController != null)
            {
                if (!_activePlayers.Contains(playerController))
                {
                    _activePlayers.Add(playerController);

                    if (playerController.HasInputAuthority || playerController.Runner.IsServer)
                    {
                        Vector3 bounceDir = (other.transform.position - transform.position);
                        bounceDir.y = 0f; 
                        bounceDir = bounceDir.normalized;
                        bounceDir.y = 0.4f; 

                        playerController.ApplyNetworkKnockback(bounceDir.normalized, bounceForce, stunTime);
                    }

                    // Автоматическая перезарядка бампера по времени отскока!
                    StartCoroutine(ReleasePlayerRoutine(playerController, stunTime));
                }
            }
        }
    }

    private IEnumerator ReleasePlayerRoutine(NetworkPlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (_activePlayers.Contains(player))
        {
            _activePlayers.Remove(player);
            Debug.Log($"[Бампер] Игрок отлетел на безопасное расстояние. Перезарядка завершена.");
        }
    }
}
