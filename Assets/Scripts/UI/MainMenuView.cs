using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuView : MonoBehaviour
{
    public void OnPlayBtnClick()
    {
        // Загружаем сцену с игрой
        Debug.Log("Загрузка сцены с игрой...");
        SceneManager.LoadSceneAsync("GameScene");        
    }
}
