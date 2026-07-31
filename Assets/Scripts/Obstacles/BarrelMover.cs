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

        // Создаем последовательность анимаций (Sequence)
        Sequence barrelSequence = DOTween.Sequence();

        // 1. Подъем вверх
        barrelSequence.Append(transform.DOLocalMoveY(_startPosition.y + heightOffset, upDuration).SetEase(Ease.OutQuad));
        
        // 2. Ожидание наверху
        barrelSequence.AppendInterval(stayDuration);
        
        // 3. Спуск вниз под воду
        barrelSequence.Append(transform.DOLocalMoveY(_startPosition.y, downDuration).SetEase(Ease.InQuad));
        
        // 4. Ожидание под водой перед следующим циклом
        barrelSequence.AppendInterval(hideDuration);

        // Настраиваем бесконечный цикл всей последовательности
        barrelSequence.SetLoops(-1);
        
        // Синхронизируем с физикой Unity, чтобы игрок устойчиво стоял на бочке
        barrelSequence.SetUpdate(UpdateType.Fixed);

        // Задаем индивидуальную задержку старта, чтобы бочки двигались не одновременно
        if (startDelay > 0f)
        {
            barrelSequence.PrependInterval(startDelay);
        }
    }
}
