using UnityEngine;
using DG.Tweening;

public class NetworkTrampoline : MonoBehaviour
    {
        [Header("Настройки прыжка")]
        [SerializeField] private float bounceForce = 18f; // Сила подброса вверх

        [Header("Анимация батута (DOTween)")]
        [SerializeField] private Transform visualModel;     // Сюда перетащите дочерний 3D-меш батута
        [SerializeField] private float squashScaleY = 0.43f; // Насколько сильно сжимается
        [SerializeField] private float duration = 0.1f;      // Скорость сжатия

        private Vector3 _originalScale;
        private bool _isAnimating = false;

        private void Start()
        {
            if (visualModel == null) visualModel = transform;
            _originalScale = visualModel.localScale;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var playerController = other.GetComponent<NetworkPlayerController>();
                
                if (playerController != null)
                {
                    if (playerController.HasInputAuthority || playerController.Runner.IsServer)
                    {
                        // Вызываем специальный сетевой метод прыжка вверх
                        playerController.ApplyNetworkTrampolineBounce(bounceForce);
                        
                        Debug.Log($"[Батут] Локальный толчок вверх выполнен с силой: {bounceForce}");
                    }

                    // ЗАПУСКАЕМ ЛОКАЛЬНЫЙ SQUASH ПО DOTWEEN
                    // Анимация проиграется на ПК у того, кто наступил, без отправки тяжелых пакетов по сети
                    PlayBounceAnimation();
                }
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
