using UnityEngine;
using DG.Tweening;  

public class Rotator : MonoBehaviour
{

    [SerializeField] private Ease easeType;
    [SerializeField] private float duration;
    [SerializeField] private Vector3 from;
    [SerializeField] private Vector3 to;
    
    void Start()
    {
        transform.localRotation = Quaternion.Euler(from);

        transform.DOLocalRotate(to, duration)
            .SetEase(easeType) // Плавное замедление на концах траектории (как у маятника)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(UpdateType.Fixed);
    }
}
