using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class NetworkTrampoline : MonoBehaviour
{
    [Header("Настройки прыжка")]
    [SerializeField] private float bounceForce = 18f; 
    [SerializeField] private float stunTime = 0.45f;    

    [Header("Анимация батута (DOTween)")]
    [SerializeField] private Transform visualModel;     
    [SerializeField] private float squashScaleY = 0.43f; 
    [SerializeField] private float duration = 0.1f;      

    private Vector3 _originalScale;
    private bool _isAnimating = false;

    // Список блокировки игроков
    private HashSet<NetworkPlayerController> _activePlayers = new HashSet<NetworkPlayerController>();

    private void Start()
    {
        if (visualModel == null) visualModel = transform;
        _originalScale = visualModel.localScale;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            
            if (playerController != null)
            {
                if (!_activePlayers.Contains(playerController))
                {
                    _activePlayers.Add(playerController);

                    if (playerController.HasInputAuthority || playerController.Runner.IsServer)
                    {
                        playerController.ApplyNetworkTrampolineBounce(bounceForce, stunTime);
                        PlayBounceAnimation();
                    }

                    // ЖЕЛЕЗОБЕТОННЫЙ МАНЕВР: Запускаем таймер автоматического удаления из черного списка!
                    // Он сработает гарантированно, даже если корова улетела в космос за 1 кадр
                    StartCoroutine(ReleasePlayerRoutine(playerController, stunTime));
                }
            }
        }
    }

    private IEnumerator ReleasePlayerRoutine(NetworkPlayerController player, float delay)
    {
        // Ждем ровно столько, сколько корова находится в фазе неуправляемого взлета
        yield return new WaitForSeconds(delay);
        
        if (_activePlayers.Contains(player))
        {
            _activePlayers.Remove(player);
            Debug.Log($"[Батут] Игрок автоматически удален из блокировки по таймеру. Батут готов!");
        }
    }

    private void PlayBounceAnimation()
    {
        if (_isAnimating || visualModel == null) return;
        _isAnimating = true;

        Sequence squashSeq = DOTween.Sequence();
        squashSeq.Append(visualModel.DOScaleY(_originalScale.y * squashScaleY, duration).SetEase(Ease.OutQuad));
        squashSeq.Append(visualModel.DOScaleY(_originalScale.y, duration * 2f).SetEase(Ease.OutElastic));
        squashSeq.OnComplete(() => _isAnimating = false);
    }
}
