using UnityEngine;


namespace NonameGame
{
    public class NetworkViewFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float positionThreshold = 0.005f;
        [Tooltip("Скорость плавного следования за физическим телом. Чем выше, тем отзывчивее графика.")]
        [SerializeField] private float positionLerpSpeed = 25f;
        [SerializeField] private float rotationLerpSpeed = 15f;

        void Start()
        {
            transform.SetParent(null);
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            float distanceToTarget = Vector3.Distance(transform.position, _target.position);

            if (distanceToTarget > positionThreshold)
            {
                if (distanceToTarget > 3f)
                {
                    transform.position = _target.position;
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * positionLerpSpeed);
                }
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, _target.rotation, Time.deltaTime * rotationLerpSpeed);            
        }
    }
}
