using Unity.Cinemachine;
using UnityEngine;

public class NetworkPuncher : MonoBehaviour
{
   [SerializeField] private float punchForce = 30f;
    [SerializeField] private CinemachineCamera _camera;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var ragdoll = other.GetComponent<NetworkPlayerRagdoll>();
                if (ragdoll != null)
                {
                    Vector3 punchDirection = transform.forward;
                    punchDirection.y = 0.5f; // Подбрасываем вверх

                    // Метод сработает одновременно у всех клиентов, и персонаж эпично улетит у каждого на экране
                    ragdoll.RPC_ApplyRagdollImpulse(punchDirection.normalized, punchForce, _camera);
                    
                    Debug.Log("[Перчатка] Сетевой RPC импульс рэгдолла отправлен!");
                }
            }
        }
    }
