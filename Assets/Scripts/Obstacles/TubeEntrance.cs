using UnityEngine;
using UnityEngine.Splines;

public class TubeEntrance : MonoBehaviour
{
    [SerializeField] private SplineContainer _spline;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<TubeTraveler>() != null) return;

            var ragdoll = other.GetComponent<PlayerRagdoll>();
            if (ragdoll == null)
                return;
            
            ragdoll.ToggleRagdoll(true);

            var traveler = other.gameObject.AddComponent<TubeTraveler>();
            traveler.SetupPath(_spline, ragdoll);
        }
    }
}
