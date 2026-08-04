using UnityEngine;

public class FollowPositionOnly : MonoBehaviour
{
    public Transform targetHips; // Сюда перетащим mixamorig:Hips в инспекторе

    void FixedUpdate()
    {
        if (targetHips != null)
        {
            transform.position = targetHips.position;
        }
    }
}
