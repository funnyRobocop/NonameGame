using System.Collections;
using System.Collections.Generic;
using Fusion;
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

                    // ЖЕЛЕЗОБЕТОННЫЙ МАНЕВР FUSION 2.1:
                    // Импульс применяет ТАКЖЕ и Клиент, у которого есть права управления (HasInputAuthority)!
                    // Это позволит клиенту мгновенно предсказать полет на своем экране без задержек сети
                    if (playerController.HasInputAuthority || playerController.Runner.IsServer)
                    {
                        Vector3 bounceDir = (other.transform.position - transform.position);
                        bounceDir.y = 0f; 
                        bounceDir = bounceDir.normalized;
                        bounceDir.y = 0.4f; 

                        Vector3 finalKnockbackVector = bounceDir.normalized * bounceForce;

                        // Вызываем метод БЕЗ всяких RPC — напрямую активируем физический буфер кадра!
                        playerController.ApplyLocalPredictedKnockback(finalKnockbackVector, stunTime);
                    }

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
        }
    }
}
