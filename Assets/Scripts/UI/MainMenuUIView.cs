using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIView : MonoBehaviour
{
    void Start()
    {
        CursorController.LockCursor(false);
    }

    public void OnPlayBtnClick()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void OnShopButtonClicked()
    {
        SceneManager.LoadScene("ShopScene");
    }
}
