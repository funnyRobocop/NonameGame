using UnityEngine;
using Fusion;
using Zenject;
using UnityEngine.Splines;

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
            var ragdoll = other.GetComponent<NetworkPlayerRagdoll>();

            if (playerController != null && ragdoll != null)
            {
                // ВАЖНО: запуск полета в трубе инициирует ТОЛЬКО тот клиент, который управляет коровой (Input Authority)
                // Это уберет задержки — корова влетит в трубу мгновенно в момент касания
                if (playerController.HasInputAuthority)
                {
                    // 1. Включаем сетевой рэгдолл через созданный ранее RPC (силу ставим 0, так как пинать не нужно)
                    ragdoll.RPC_ApplyRagdollImpulse(Vector3.zero, 20f, _cameraIndex);

                    // 2. Вешаем сетевой скрипт путешественника
                    var traveler = other.gameObject.AddComponent<NetworkTubeTraveler>();
                    traveler.SetupPath(_spline, ragdoll, _speed);
                }
            }
        }
    }
}
