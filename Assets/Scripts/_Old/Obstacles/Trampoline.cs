using UnityEngine;
using DG.Tweening; // Подключаем DOTween

public class Trampoline : MonoBehaviour
{
    [Header("Настройки прыжка")]
    [SerializeField] private float bounceForce;

    [Header("Анимация батута (DOTween)")]
    [SerializeField] private float squashScaleY;
    [SerializeField] private float duration;

    private Vector3 _originalScale;
    private bool _isBouncing = false;

    void Start()
    {
        _originalScale = transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_isBouncing)
        {
            var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                _isBouncing = true;

                controller.BouncePlayer(bounceForce);

                transform.DOScaleY(_originalScale.y * squashScaleY, duration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        transform.DOScaleY(_originalScale.y * 1.2f, duration * 0.8f)
                            .SetEase(Ease.OutElastic)
                            .OnComplete(() =>
                            {
                                transform.DOScaleY(_originalScale.y, duration)
                                    .OnComplete(() => _isBouncing = false);
                            });
                    });
            }
        }
    }
}
