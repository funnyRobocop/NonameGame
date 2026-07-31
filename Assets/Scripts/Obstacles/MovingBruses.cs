using UnityEngine;

public class MovingBruses : MonoBehaviour
{
    [SerializeField] private Vector3 _moving;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.SetForcedMoving(_moving);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.SetForcedMoving(Vector3.zero);
            }
        }
    }
}
