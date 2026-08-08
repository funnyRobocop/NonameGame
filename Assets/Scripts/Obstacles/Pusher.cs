using UnityEngine;

public class Pusher : MonoBehaviour
{
    [SerializeField] private float pushForce;
    [SerializeField] private float yForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var ragdollComponent = other.GetComponent<PlayerRagdoll>();
            if (ragdollComponent != null)
            {                
                // Считаем вектор удара
                var strikeDirection = (other.transform.position - transform.position).normalized;
                strikeDirection.y = yForce;

                // Активируем падение
                ragdollComponent.ApplyRagdollImpulse(strikeDirection, pushForce);
            }
        }
    }
}
