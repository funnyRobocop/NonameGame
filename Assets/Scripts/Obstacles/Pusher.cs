using UnityEngine;

public class Pusher : MonoBehaviour
{
    [SerializeField] private float pushForce;
    [SerializeField] private float yForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            /*var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                pushDir.y = yForce; 
                
                Debug.Log($"Pushing player with force {pushForce}");
                controller.AddKnockback(pushDir, pushForce);
            }*/

            var ragdollComponent = other.GetComponent<PlayerRagdoll>();
            if (ragdollComponent != null)
            {                
                // Считаем вектор удара
                Vector3 strikeDirection = (other.transform.position - transform.position).normalized;
                strikeDirection.y = 1f; // Подбрасываем корову повыше в воздух!

                float strikeForce = 10f; // Сила физического удара кувалды

                // Активируем падение
                ragdollComponent.ApplyRagdollImpulse(strikeDirection, strikeForce);
            }
        }
    }
}
