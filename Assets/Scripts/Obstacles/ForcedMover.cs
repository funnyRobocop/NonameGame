using UnityEngine;

public class ForcedMover : MonoBehaviour
{
    [SerializeField] private Vector3 _moving;
    [SerializeField] private bool _changeCam;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.AddForcedMoving(_moving);
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
                controller.AddForcedMoving(_moving * -1f);
            }
        }
    }
}
