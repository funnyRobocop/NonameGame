using UnityEngine;
using TMPro;
using Zenject;

public class LevelFinishView : MonoBehaviour
{
    [SerializeField] private GameObject _levelFinishPanel;
    [SerializeField] private TextMeshProUGUI _rewardText;
    private GameDataModel _model;

    private void Awake()
    {
        gameObject.SetActive(false);
        _levelFinishPanel.SetActive(false);
    }

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _levelFinishPanel.SetActive(true);

        if (_rewardText != null)
        {
            _rewardText.text = $"+{_model.Coins.Value} Coins\n+{_model.Score.Value} Score";
        }
    }
}
