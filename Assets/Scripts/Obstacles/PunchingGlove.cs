using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PunchingGlove : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    [SerializeField] private Animator animator;

    [Header("Тайминги Анимации")]
    [SerializeField] private string triggerName;

    [Header("Настройки удара (DOTween)")]
    [SerializeField] private float punchDistance; 
    [SerializeField] private float punchSpeed;  
    [SerializeField] private float stayDuration; 
    [SerializeField] private float returnSpeed;    
    
    [Header("Тайминги цикла")]
    [SerializeField] private float restDuration;   
    [SerializeField] private float startDelay;     

    private Vector3 _startLocalPos;
    private bool _isPunching;

    void Start()
    {
        _startLocalPos = transform.localPosition;

        StartCoroutine(CycleRoutine());
    }

    private IEnumerator CycleRoutine()
    {
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            if (!_isPunching)
            {
                if (animator != null) animator.SetTrigger(triggerName);
            }

            // Ждем завершения всего цикла (включая отдых), прежде чем запустить анимацию снова
            yield return new WaitForSeconds(restDuration + punchSpeed + stayDuration + returnSpeed);
        }
    }

    //метод мы вызовем через Animation Event
    public void ExecutePhysicalPunch()
    {
        if (_isPunching) return;
        StartCoroutine(PunchFlyRoutine());
    }

    private IEnumerator PunchFlyRoutine()
    {
        _isPunching = true;

        Sequence punchSeq = DOTween.Sequence();
        punchSeq.Append(transform.DOLocalMoveZ(_startLocalPos.z + punchDistance, punchSpeed).SetEase(Ease.OutQuad));
        punchSeq.AppendInterval(stayDuration);
        punchSeq.Append(transform.DOLocalMoveZ(_startLocalPos.z, returnSpeed).SetEase(Ease.InOutSine));
        punchSeq.SetUpdate(UpdateType.Fixed);

        yield return punchSeq.WaitForCompletion();
        
        _isPunching = false;
    }
}
