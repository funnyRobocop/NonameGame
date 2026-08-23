using UnityEngine;
using Fusion;

namespace NonameGame
{
    public class NetworkFinishZone : NetworkBehaviour
    {
        [Networked] private int _finishedPlayersCount { get; set; }

        public override void Spawned()
        {
            if (Runner.IsServer)
            {
                _finishedPlayersCount = 0;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Runner.IsServer) return;

            if (other.CompareTag("Player"))
            {
                var playerController = other.GetComponent<NetworkPlayerController>();
                
                if (playerController != null && !playerController.IsFinished)
                {
                    _finishedPlayersCount++;

                    int assignedPlace = _finishedPlayersCount;
                    
                    Debug.Log($"[МЕНЕДЖЕР ФИНИША] Игрок {other.gameObject.name} занял {assignedPlace} место!");

                    playerController.SetPlayerFinished(assignedPlace);
                }
            }
        }
    }
}
