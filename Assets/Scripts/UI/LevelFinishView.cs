using UnityEngine;
using TMPro;
using Zenject;

public class LevelFinishView : MonoBehaviour
{
    [SerializeField] private GameObject _levelFinishPanel;
    [SerializeField] private TextMeshProUGUI _rewardText;
    private GameDataModel _model;

    public bool IsVisible => gameObject.activeSelf;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    public void Show()
    {
        Debug.Log("Показать панель завершения уровня");
        gameObject.SetActive(true);
        _levelFinishPanel.SetActive(true);

        if (_rewardText != null)
        {
            _rewardText.text = $"+{_model.Coins.Value} Coins\n+{_model.Score.Value} Score";
        }
    }
}
