using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    private CharacterController _controller;
    private Animator _animator;
    private Rigidbody[] _ragdollRigidbones;
    private Collider[] _ragdollColliders;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        
        // Находим все твердые тела и коллайдеры на костях персонажа
        _ragdollRigidbones = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = GetComponentsInChildren<Collider>();

        // При старте игры выключаем Ragdoll, чтобы персонаж бегал нормально
        ToggleRagdoll(false);
    }

    // Главный метод включения / выключения физического падения
    public void ToggleRagdoll(bool isRagdoll)
    {
        // 1. Отключаем/включаем логику управления и анимации
        if (_controller != null) _controller.enabled = !isRagdoll;
        if (_animator != null) _animator.enabled = !isRagdoll;
        
        // Отключаем сам скрипт перемещения
        var movementScript = GetComponent<ThirdPersonController>();
        if (movementScript != null) movementScript.enabled = !isRagdoll;

        // 2. Включаем/выключаем физику на каждой кости скелета
        foreach (var rb in _ragdollRigidbones)
        {
            // Если Ragdoll выключен, кости должны быть Kinematic (не падать сами по себе)
            rb.isKinematic = !isRagdoll;
        }

        foreach (var col in _ragdollColliders)
        {
            // Отключаем коллизии костей, когда игрок просто бегает, чтобы они не мешали основному коллайдеру
            if (col.gameObject != this.gameObject) // Не трогаем главный коллайдер игрока
            {
                col.enabled = isRagdoll;
            }
        }
    }

    // Метод, который кувалда будет вызывать для сильного удара
    public void ApplyRagdollImpulse(Vector3 forceDirection, float forceMagnitude)
    {
        ToggleRagdoll(true);

        // Ищем кость таза (обычно она первая в списке) и прикладываем импульс к ней
        if (_ragdollRigidbones.Length > 0)
        {
            // Прикладываем силу ко всем костям сразу для сочного эффекта отлета
            foreach (var rb in _ragdollRigidbones)
            {
                rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
            }
        }

        // Запускаем таймер автоматического подъема через 3 секунды
        Invoke(nameof(StandUp), 3f);
    }

    private void StandUp()
    {
        Debug.Log("Standing up from ragdoll " + Time.time);
        ToggleRagdoll(false);

        GetComponent<PlayerInit>().RespawnPlayer();
    }
}
