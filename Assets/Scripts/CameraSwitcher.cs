using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera[] _allRagdollCameras;

    public void SwitchRagdollCameras( CinemachineCamera _activeCamera)
    {
        ResetRagdollCameras();

        if (_activeCamera != null) _activeCamera.Priority = 20;
    }

    public void ResetRagdollCameras()
    {
        foreach (var cam in _allRagdollCameras)
        {
            if (cam != null) cam.Priority = 0;
        }
    }
}
