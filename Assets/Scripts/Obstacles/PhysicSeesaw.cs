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
            // Берем точку, где сейчас стоят ноги игрока
            Vector3 forcePosition = other.transform.position;
            
            // Направление силы — строго вниз (имитация гравитации и веса)
            Vector3 forceDirection = Vector3.down * playerWeightForce;

            // Прикладываем силу к качелям в конкретной точке
            _rigidbody.AddForceAtPosition(forceDirection, forcePosition, ForceMode.Force);
        }
    }
}
