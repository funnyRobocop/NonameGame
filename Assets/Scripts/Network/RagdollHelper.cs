using UnityEngine;
using System.Collections.Generic;

public class RagdollHelper : MonoBehaviour
{
    public bool ragdolled
    {
        get => state != RagdollState.animated;
        set
        {
            if (value)
            {
                if (state == RagdollState.animated)
                {
                    SetKinematic(false);
                    anim.enabled = false;
                    state = RagdollState.ragdolled;
                }
            }
            else
            {
                if (state == RagdollState.ragdolled)
                {
                    SetKinematic(true);
                    ragdollingEndTime = Time.time;
                    anim.enabled = true;
                    state = RagdollState.blendToAnim;

                    // Сохраняем позу ragdoll
                    foreach (BodyPart b in bodyParts)
                    {
                        b.storedRotation = b.transform.rotation;
                        b.storedPosition = b.transform.position;
                    }

                    ragdolledFeetPosition = 0.5f *
                        (anim.GetBoneTransform(HumanBodyBones.LeftToes).position +
                         anim.GetBoneTransform(HumanBodyBones.RightToes).position);

                    ragdolledHeadPosition = anim.GetBoneTransform(HumanBodyBones.Head).position;
                    ragdolledHipPosition = anim.GetBoneTransform(HumanBodyBones.Hips).position;

                    // Определяем, на спине или на животе
                    if (anim.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0f)
                        anim.SetBool("GetUpFromBack", true);
                    else
                        anim.SetBool("GetUpFromBelly", true);
                }
            }
        }
    }

    enum RagdollState { animated, ragdolled, blendToAnim }
    RagdollState state = RagdollState.animated;

    public float ragdollToMecanimBlendTime = 0.5f;
    float mecanimToGetUpTransitionTime = 0.05f;
    float ragdollingEndTime = -100f;

    class BodyPart
    {
        public Transform transform;
        public Vector3 storedPosition;
        public Quaternion storedRotation;
    }

    List<BodyPart> bodyParts = new List<BodyPart>();
    Vector3 ragdolledHipPosition, ragdolledHeadPosition, ragdolledFeetPosition;
    Animator anim;

    void SetKinematic(bool value)
    {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = value;
    }

    void Start()
    {
        SetKinematic(true);

        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            bodyParts.Add(new BodyPart { transform = t });
        }

        anim = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        // Сбрасываем триггеры, чтобы анимация не зацикливалась
        anim.SetBool("GetUpFromBelly", false);
        anim.SetBool("GetUpFromBack", false);

        if (state != RagdollState.blendToAnim)
            return;

        // Первые кадры — подстраиваем корневой Transform под ragdoll
        if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime)
        {
            Vector3 animatedToRagdolled = ragdolledHipPosition - anim.GetBoneTransform(HumanBodyBones.Hips).position;
            Vector3 newRootPosition = transform.position + animatedToRagdolled;

            // Ставим на землю
            RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition + Vector3.up * 0.5f, Vector3.down), 2f);
            newRootPosition.y = 0f;
            foreach (RaycastHit hit in hits)
            {
                if (!hit.transform.IsChildOf(transform))
                    newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
            }
            transform.position = newRootPosition;

            // Поворачиваем персонажа в сторону, куда он смотрел лёжа
            Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
            ragdolledDirection.y = 0f;

            Vector3 meanFeet = 0.5f *
                (anim.GetBoneTransform(HumanBodyBones.LeftFoot).position +
                 anim.GetBoneTransform(HumanBodyBones.RightFoot).position);

            Vector3 animatedDirection = anim.GetBoneTransform(HumanBodyBones.Head).position - meanFeet;
            animatedDirection.y = 0f;

            transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdolledDirection.normalized);
        }

        // Бленд позы ragdoll → анимация
        float blend = 1f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
        blend = Mathf.Clamp01(blend);

        foreach (BodyPart b in bodyParts)
        {
            if (b.transform == transform) continue;

            if (b.transform == anim.GetBoneTransform(HumanBodyBones.Hips))
                b.transform.position = Vector3.Lerp(b.transform.position, b.storedPosition, blend);

            b.transform.rotation = Quaternion.Slerp(b.transform.rotation, b.storedRotation, blend);
        }

        if (blend <= 0f)
            state = RagdollState.animated;
    }
}