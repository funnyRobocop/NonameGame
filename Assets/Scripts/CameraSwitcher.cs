using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera[] _allRagdollCameras;

    public void SwitchOnRagdollCamera( CinemachineCamera _activeCamera)
    {
        SwitchOffAllRagdollCameras();

        if (_activeCamera != null) _activeCamera.Priority = 20;
    }
    
    public void SwitchOnRagdollCamera( int cameraIndex)
    {
        SwitchOffAllRagdollCameras();

        if (cameraIndex >= 0 && cameraIndex < _allRagdollCameras.Length)
        {
            _allRagdollCameras[cameraIndex].Priority = 20;
        }
    }

    public void SwitchOffAllRagdollCameras()
    {
        foreach (var cam in _allRagdollCameras)
        {
            if (cam != null) cam.Priority = 0;
        }
    }
}
