using UnityEngine;

public class TubeEntrance : MonoBehaviour
{
    [SerializeField]private GameObject _spline;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<TubeTraveler>() != null) return;

            var ragdoll = other.GetComponent<PlayerRagdoll>();
            if (ragdoll != null)
            {
                ragdoll.ToggleRagdoll(true);
            }

            var traveler = other.gameObject.AddComponent<TubeTraveler>();
            
            if (_spline != null)
            {
                traveler.SetupPath(_spline.transform);
            }
        }
    }
}
