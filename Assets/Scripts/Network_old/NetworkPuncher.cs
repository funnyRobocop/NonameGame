using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace NonameGame
{
public class NetworkPuncher : MonoBehaviour
{
    [SerializeField] private float punchForce = 30f;
    [SerializeField] private int _cameraIndex;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var networkPlayer = other.GetComponent<NetworkPlayerController>();
                
                if (networkPlayer != null)
                {
                    Vector3 punchDir = (other.transform.position - transform.position).normalized;
                    punchDir.y = 0.3f;

                    networkPlayer.RPC_ApplyServerRagdollImpulse(punchDir, punchForce, _cameraIndex);
                }
            }
        }
    }
}
