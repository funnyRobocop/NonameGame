using UnityEngine;
using Fusion;
using Zenject;
using UnityEngine.Splines;

namespace NonameGame
{
    public class NetworkTubeEntrance : MonoBehaviour
    {
        [SerializeField] private float _speed;
        [SerializeField] private SplineContainer _spline;
        [SerializeField] private int _cameraIndex;
        [Inject] private CameraSwitcher _cameraSwitcher;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Проверяем: если игрок уже летит в трубе, игнорируем
                if (other.GetComponent<NetworkTubeTraveler>() != null) return;

                // Находим сетевой контроллер персонажа
                var playerController = other.GetComponent<NetworkPlayerController>();

                if (playerController != null)
                {
                    playerController.RPC_ApplyServerRagdollImpulse(Vector3.forward, 20f, _cameraIndex);

                    // 2. Вешаем сетевой скрипт путешественника
                    var traveler = other.gameObject.AddComponent<NetworkTubeTraveler>();
                    traveler.SetupPath(_spline, playerController._ragdoll, _speed);
                }
            }
        }
    }
}
