using DG.Tweening;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private Vector3 moveOffset;
    [SerializeField] private float duration;

    void Start()
    {
        Vector3 targetPosition = transform.localPosition + moveOffset;

        transform.DOLocalMove(targetPosition, duration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(UpdateType.Fixed);
    }
}
