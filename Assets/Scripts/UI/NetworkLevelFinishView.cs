using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkLevelFinishView : MonoBehaviour
{
    [Header("Элементы UI")]
    [SerializeField] private GameObject qualifiedPanel;
    [SerializeField] private TextMeshProUGUI placeText; 
    [SerializeField] private Button leaveButton;

    private NetworkPlayerController localPlayer;

    private void Start()
    {
        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(LeaveSessionAndReturnToMenu);
        }
    }

    private void Update()
    {
        if (localPlayer == null)
            localPlayer = FindLocalPlayer();

        if (localPlayer == null)
            return;
            
        if (!localPlayer.IsFinished)
            return;
        
        if (!qualifiedPanel.activeSelf)
        {
            qualifiedPanel.SetActive(true);
            
            if (placeText != null)
            {
                placeText.text = $"МЕСТО: {localPlayer.FinishPlace}";
            }
        }
    }

    private NetworkPlayerController FindLocalPlayer()
    {
        var allPlayers = FindObjectsByType<NetworkPlayerController>();
        foreach (var player in allPlayers)
        {
            if (player.HasInputAuthority) return player;
        }
        return null;
    }
    
    private async void LeaveSessionAndReturnToMenu()
    {
        NetworkRunner runner = FindObjectsByType<NetworkRunner>().FirstOrDefault();

        if (runner != null)
        {
            Debug.Log("[Сеть] Выходим из игры. Закрываем сетевую сессию Photon...");
            
            await runner.Shutdown();
        }

        SceneManager.LoadScene(0);
    }
}