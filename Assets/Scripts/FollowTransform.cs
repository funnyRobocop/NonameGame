using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform target;
    public bool positionOnly;
    public bool isInFixedUpdate;

    void Update()
    {
        if (isInFixedUpdate)
            return;
            
        if (target != null)
        {
            transform.position = target.position;
            if (!positionOnly)
                transform.rotation = target.rotation;
        }
    }

    void FixedUpdate()
    {
        if (!isInFixedUpdate)
            return;

        if (target != null)
        {
            transform.position = target.position;
            if (!positionOnly)
                transform.rotation = target.rotation;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
