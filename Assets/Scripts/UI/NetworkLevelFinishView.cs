using TMPro;
using UnityEngine;

public class NetworkLevelFinishView : MonoBehaviour
{
    [Header("Элементы UI")]
    [SerializeField] private GameObject qualifiedPanel;
    [SerializeField] private TextMeshProUGUI placeText; 

    private NetworkPlayerController localPlayer;

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
        var allPlayers = FindObjectsByType<NetworkPlayerController>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            if (player.HasInputAuthority) return player;
        }
        return null;
    }
}