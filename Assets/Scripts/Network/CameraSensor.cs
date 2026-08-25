using Unity.Cinemachine;
using UnityEngine;

public class CameraSensor : MonoBehaviour
{
    [SerializeField] private CameraSwitcher _cameraSwitcher;

    [SerializeField] private CinemachineCamera _camera;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _cameraSwitcher.SwitchOnRagdollCamera(_camera);
        }
    }
}
