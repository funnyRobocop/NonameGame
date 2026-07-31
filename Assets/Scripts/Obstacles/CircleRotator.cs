using UnityEngine;
using DG.Tweening;

public class CircleRotator : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private float sign;

    void Start()
    {
        transform.DOLocalRotate(new Vector3(0f, sign * 360f, 0f), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental)
            .SetUpdate(UpdateType.Fixed);
    }
}
