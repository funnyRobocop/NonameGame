using UnityEngine;

public class PhysicSeesaw : MonoBehaviour
{
    [SerializeField] private float playerWeightForce;
    private Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Давим весом игрока на качели
            Vector3 forcePosition = other.transform.position;
            Vector3 forceDirection = Vector3.down * playerWeightForce;
            _rigidbody.AddForceAtPosition(forceDirection, forcePosition, ForceMode.Force);

            var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.ForceGrounded = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.ForceGrounded = true;
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
                controller.ForceGrounded = false;
            }
        }
    }
}
