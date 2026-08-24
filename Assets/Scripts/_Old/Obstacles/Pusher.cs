using Unity.Cinemachine;
using UnityEngine;

public class Pusher : MonoBehaviour
{
    [SerializeField] private float _pushForce;
    [SerializeField] private float _yForce;
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private CameraSwitcher _cameraSwitcher;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var ragdollComponent = other.GetComponent<PlayerRagdoll>();
            if (ragdollComponent != null)
            {
                var strikeDirection = (other.transform.position - transform.position).normalized;
                strikeDirection.y = _yForce;

                _cameraSwitcher.SwitchOnRagdollCamera(_camera);
                ragdollComponent.ApplyRagdollImpulse(strikeDirection, _pushForce);
            }
        }
    }
}
