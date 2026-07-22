using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopUIView : MonoBehaviour
{

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

}
