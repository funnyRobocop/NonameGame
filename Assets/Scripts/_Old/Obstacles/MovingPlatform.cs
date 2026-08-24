using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PlatformConnector platformConnector))
            {
                platformConnector.Connect(transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PlatformConnector platformConnector))
            {
                platformConnector.Disconnect(transform);
            }
        }
    }
}
