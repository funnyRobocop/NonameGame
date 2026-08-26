using UnityEngine;
using Fusion;
using System.Collections;
using PhysicsCharacterController; // Подключаем пространство имен ассета Nappin
using Zenject;

namespace NonameGame
{
    public class NetworkPlayerRagdoll : MonoBehaviour
    {
        [Header("Кости и Настройки")]
        [SerializeField] private Transform ragdollHips; 
        [SerializeField] private LayerMask groundLayer;  
        [SerializeField] private float standUpDistance = 0.3f; 

        private CameraSwitcher _cameraSwitcher;
        private PhysicsPlayerController _nappinCharacterManager; // Новая ссылка на контроллер Nappin
        private Rigidbody _rootRigidbody;               // Ссылка на корневой Rigidbody коровы
        private Animator _animator;
        private Rigidbody[] _ragdollRigidbones;
        private Collider[] _ragdollColliders;
        private Rigidbody _hipsRigidbody;
        
        private bool _isRagdollActive = false;
        private Coroutine _groundCheckCoroutine;
        public Transform HipsTransform => ragdollHips;

        [Inject]
        public void Construct(CameraSwitcher cameraSwitcher)
        {
            _cameraSwitcher = cameraSwitcher;
        }
        
        void Awake()
        {
            _animator = GetComponent<Animator>();
            _nappinCharacterManager = GetComponent<PhysicsPlayerController>();
            _rootRigidbody = GetComponent<Rigidbody>();
            
            _ragdollRigidbones = GetComponentsInChildren<Rigidbody>();
            _ragdollColliders = GetComponentsInChildren<Collider>();

            if (_cameraSwitcher == null) _cameraSwitcher = FindAnyObjectByType<CameraSwitcher>();

            if (ragdollHips != null)
            {
                _hipsRigidbody = ragdollHips.GetComponent<Rigidbody>();
            }
        }

        public void Init(PhysicsPlayerController physicsPlayerController)
        {
            _nappinCharacterManager = physicsPlayerController;
        }

        public void ApplyPhysicsRagdollImpulseLocal(Vector3 forceDirection, float forceMagnitude, int cameraIndex)
        {
            // 1. Включаем режим рэгдолла у всех на экранах локально
            LocalToggleRagdoll(true);

            // 2. Прикладываем физический импульс к костям
            if (_ragdollRigidbones.Length > 0)
            {
                foreach (var rb in _ragdollRigidbones)
                {
                    rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
                }
            }

            // 3. Отслеживание земли (Проверяем права через ссылку на контроллер Nappin, который лежит на сетевом корне!)
            if (_nappinCharacterManager != null && _nappinCharacterManager.HasInputAuthority)
            {
                if (_cameraSwitcher != null) _cameraSwitcher.SwitchOnRagdollCamera(cameraIndex);
                if (_groundCheckCoroutine != null) StopCoroutine(_groundCheckCoroutine);
                _groundCheckCoroutine = StartCoroutine(CheckForGroundLanding());
            }
        }

        public void LocalToggleRagdoll(bool isRagdoll)
        {
            _isRagdollActive = isRagdoll;

            // Выключаем/Включаем мозг ассета Nappin и аниматор
            if (_nappinCharacterManager != null) _nappinCharacterManager.enabled = !isRagdoll;
            if (_animator != null) _animator.enabled = !isRagdoll;

            // ====================================================================
            // КРИТИЧЕСКИЙ ШАГ: УПРАВЛЕНИЕ КОРНЕВЫМ RIGIDBODY
            // ====================================================================
            if (_rootRigidbody != null)
            {
                if (isRagdoll)
                {
                    // Когда рэгдолл включается, мы делаем корень КИНЕМАТИЧЕСКИМ.
                    // Он больше не падает, не толкается и НЕ КОНФЛИКТУЕТ с летящим тазом (Hips)!
                    _rootRigidbody.linearVelocity = Vector3.zero;
                    _rootRigidbody.angularVelocity = Vector3.zero;
                    _rootRigidbody.isKinematic = true;
                }
                else
                {
                    // Когда встаем на ноги — возвращаем корню честную динамическую физику
                    _rootRigidbody.isKinematic = false;
                }
            }

            // Настройка физики костей
            foreach (var rb in _ragdollRigidbones)
            {
                // Защищаем корень от переключения, меняем только кости рэгдолла
                if (rb.gameObject != this.gameObject)
                {
                    rb.isKinematic = !isRagdoll;
                }
            }

            foreach (var col in _ragdollColliders)
            {
                // Главную капсулу коллизий корня оставляем включенной ВСЕГДА, 
                // а кости активируем только в фазе рэгдолла
                if (col.gameObject != this.gameObject) col.enabled = isRagdoll;
            }

            if (isRagdoll)
            {           
                if (_groundCheckCoroutine != null) StopCoroutine(_groundCheckCoroutine);
                _groundCheckCoroutine = StartCoroutine(CheckForGroundLanding());
            }
            else
            {
                if (_cameraSwitcher != null) _cameraSwitcher.SwitchOffAllRagdollCameras();
            }
        }

        private IEnumerator CheckForGroundLanding()
        {
            // Небольшая задержка, чтобы корова успела отлететь от бампера и не встала в ту же секунду
            yield return new WaitForSeconds(0.5f);

            while (_isRagdollActive)
            {
                if (ragdollHips != null && _hipsRigidbody != null)
                {
                    Ray ray = new Ray(ragdollHips.position, Vector3.down);
                    if (Physics.Raycast(ray, standUpDistance + 0.4f, groundLayer))
                    {
                        // Если таз летит медленно и коснулся земли — даем команду встать
                        if (_hipsRigidbody.linearVelocity.magnitude < 1.8f) 
                        {
                            StandUp();
                            yield break;
                        }
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }

        private void StandUp()
        {
            if (_groundCheckCoroutine != null) StopCoroutine(_groundCheckCoroutine);

            if (ragdollHips != null)
            {
                Vector3 targetPosition = ragdollHips.position;
                
                RaycastHit hit;
                if (Physics.Raycast(ragdollHips.position, Vector3.down, out hit, 4f, groundLayer))
                {
                    targetPosition.y = hit.point.y + 0.05f;
                }
                else
                {
                    targetPosition.y += 0.1f;
                }

                // КРИТИЧЕСКИЙ ШАГ FUSION: Сначала возвращаем корень из кинематики в динамику, 
                // переносим его в точку падения таза, а Network Transform плавно обновит это у клиентов!
                if (_rootRigidbody != null) _rootRigidbody.isKinematic = false;
                
                transform.position = targetPosition;

                Vector3 forwardDirection = ragdollHips.forward;
                forwardDirection.y = 0; 
                if (forwardDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(forwardDirection);
                }
            }

            LocalToggleRagdoll(false);
        }
    }
}
