using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

public class TubeEntrance : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private SplineContainer _spline;
    [SerializeField] private CinemachineCamera _camera;
    [Inject] private CameraSwitcher _cameraSwitcher;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<TubeTraveler>() != null) return;

            var ragdoll = other.GetComponent<PlayerRagdoll>();
            if (ragdoll == null)
                return;
            
            _cameraSwitcher.SwitchRagdollCameras(_camera);
            ragdoll.ToggleRagdoll(true);

            var traveler = other.gameObject.AddComponent<TubeTraveler>();
            traveler.SetupPath(_spline, ragdoll, _speed);
        }
    }
}
