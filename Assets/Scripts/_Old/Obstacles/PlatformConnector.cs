using UnityEngine;

public class PlatformConnector : MonoBehaviour
{
    private Transform _currentPlatform;

    public void Connect(Transform platformTransform)
    {
        _currentPlatform = platformTransform;
        transform.SetParent(platformTransform);
    }
    public void Disconnect(Transform platformTransform)
    {
        if (_currentPlatform != platformTransform) return;
        _currentPlatform = null;
        transform.SetParent(null);
    }
}
