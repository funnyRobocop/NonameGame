using UnityEngine;
using StarterAssets;

public class Pusher : MonoBehaviour
{
    [SerializeField] private float pushForce;
    [SerializeField] private float yForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                pushDir.y = yForce; 
                
                Debug.Log($"Pushing player with force {pushForce}");
                controller.AddKnockback(pushDir, pushForce);
            }
        }
    }
}
