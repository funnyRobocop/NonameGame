using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Collections;

public class NetworkMover : NetworkBehaviour
{
    [Header("Настройки перемещения")]
    [SerializeField] private Vector3 moveOffset; 
    [SerializeField] private float speed;                             
    [SerializeField] private float timeOffset;                        

    [Header("Настройки удара ловушки")]
    [SerializeField] private float pushForce;
    [SerializeField] private float stunDuration;

    private Vector3 _startWorldPos;
    private Rigidbody _rigidbody;
    private Vector3 _lastVelocity = Vector3.zero;
    private Vector3 _lastPosition = Vector3.zero;

    // Список для защиты от многократных ударов за один проход стены
    private HashSet<NetworkPlayerController> _hitPlayers = new HashSet<NetworkPlayerController>();

    public override void Spawned()
    {
        _startWorldPos = transform.position;        
        _rigidbody = GetComponent<Rigidbody>();
        _lastPosition = transform.position;
    }

    public override void FixedUpdateNetwork()
    {
        if (_rigidbody == null) return;

        if (Runner.IsServer)
        {
            float syncedTime = Runner.SimulationTime + timeOffset;

            float pingPong = Mathf.PingPong(syncedTime * speed, 1f);
            float smoothPingPong = Mathf.SmoothStep(0f, 1f, pingPong);
            Vector3 targetWorldPosition = _startWorldPos + (moveOffset * smoothPingPong);

            Vector3 requiredVelocity = (targetWorldPosition - _rigidbody.position) / Runner.DeltaTime;
            _rigidbody.linearVelocity = requiredVelocity;

            _lastVelocity = (transform.position - _lastPosition) / Runner.DeltaTime;
            _lastPosition = transform.position;
        }
    }

    /*private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            
            if (playerController != null)
            {
                // Толкаем корову строго один раз за удар
                if (!_hitPlayers.Contains(playerController))
                {
                    _hitPlayers.Add(playerController);

                    // Импульс рассчитывает только Сервер и передает клиенту через наш готовый RPC
                    if (Runner.IsServer)
                    {
                        // Направление толчка берем от направления движения самого барьера!
                        // Куда едет стена — туда и отлетает корова
                        Vector3 pushDir = _lastVelocity.normalized;
                        
                        // Если барьер на долю секунды застыл в крайней точке, толкаем просто вбок от стены
                        if (pushDir.magnitude < 0.1f)
                        {
                            pushDir = moveOffset.normalized;
                        }

                        pushDir.y = 0.35f; // Добавляем легкий подброс вверх для сочности

                        // Вызываем наш проверенный, рабочий RPC-метод из контроллера коровы!
                        playerController.RPC_ApplyServerKnockback(pushDir.normalized * pushForce, stunDuration);
                    }

                    // Автоматически перезаряжаем триггер стены через время оглушения
                    StartCoroutine(ReleasePlayerRoutine(playerController, stunDuration));
                }
            }
        }
    }

    private IEnumerator ReleasePlayerRoutine(NetworkPlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_hitPlayers.Contains(player))
        {
            _hitPlayers.Remove(player);
        }
    }*/
}