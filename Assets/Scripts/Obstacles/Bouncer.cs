using UnityEngine;

public class Bouncer : MonoBehaviour
{
    [SerializeField] private float pushForce;
    [SerializeField] private float yForce;

    public bool CanPushPlayer { get; set; } = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!CanPushPlayer) return;

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
