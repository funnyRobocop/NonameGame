using UnityEngine;
using DG.Tweening; // Не забываем DOTween

public class BarrelMover : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float heightOffset = 3f;  // На сколько метров вверх поднимается бочка
    [SerializeField] private float upDuration = 1.5f;   // Время подъема
    [SerializeField] private float stayDuration = 2f;   // Сколько времени бочка стоит наверху
    [SerializeField] private float downDuration = 1.5f; // Время спуска
    [SerializeField] private float hideDuration = 2f;   // Сколько времени бочка «ждет» под водой

    [Header("Очередность (Задержка)")]
    public float startDelay = 0f; // Задержка перед самым первым запуском

    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.localPosition;

        Sequence barrelSequence = DOTween.Sequence();

        barrelSequence.Append(transform.DOLocalMoveY(_startPosition.y + heightOffset, upDuration).SetEase(Ease.OutQuad));
        
        barrelSequence.AppendInterval(stayDuration);
        
        barrelSequence.Append(transform.DOLocalMoveY(_startPosition.y, downDuration).SetEase(Ease.InQuad));
        
        barrelSequence.AppendInterval(hideDuration);

        barrelSequence.SetLoops(-1);
        
        barrelSequence.SetUpdate(UpdateType.Fixed);

        if (startDelay > 0f)
        {
            barrelSequence.PrependInterval(startDelay);
        }
    }
}
