using UnityEngine;
using UnityEngine.InputSystem;

public class GameUIView : MonoBehaviour
{

    [SerializeField] private LevelFinishView _levelFinishView;

    void Start()
    {
        CursorController.LockCursor(true);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!_levelFinishView.IsVisible)
            {
                CursorController.LockCursor(false);
            }
        }
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        { 
            if (!_levelFinishView.IsVisible)
            {
                CursorController.LockCursor(true);
            }
        }
    }

    public void ShowLevelFinishView()
    {
        if (_levelFinishView != null)
        {
            _levelFinishView.Show();
            CursorController.LockCursor(false);
        }
    }
}
