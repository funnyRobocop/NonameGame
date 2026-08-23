using UnityEngine;
using UnityEngine.Events;
using System;
using Fusion; // ОБЯЗАТЕЛЬНО подключаем Photon Fusion

namespace PhysicsCharacterController
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    // Изменяем наследование с MonoBehaviour на NetworkBehaviour!
    public class CharacterManager : NetworkBehaviour
    {
        [Header("Movement specifics")]
        [Tooltip("Layers where the player can stand on")]
        [SerializeField] LayerMask groundMask;
        [Tooltip("Base player speed")]
        public float movementSpeed = 14f;
        [Range(0f, 1f)]
        [Tooltip("Minimum input value to trigger movement")]
        public float crouchSpeedMultiplier = 0.248f;
        [Range(0.01f, 0.99f)]
        [Tooltip("Minimum input value to trigger movement")]
        public float movementThrashold = 0.01f;
        [Space(10)]

        [Tooltip("Speed up multiplier")]
        public float dampSpeedUp = 0.2f;
        [Tooltip("Speed down multiplier")]
        public float dampSpeedDown = 0.1f;


        [Header("Jump and gravity specifics")]
        [Tooltip("Jump velocity")]
        public float jumpVelocity = 20f;
        [Tooltip("Multiplier applied to gravity when the player is falling")]
        public float fallMultiplier = 1.7f;
        [Tooltip("Multiplier applied to gravity when the player is holding jump")]
        public float holdJumpMultiplier = 5f;
        [Range(0f, 1f)]
        [Tooltip("Player friction against floor")]
        public float frictionAgainstFloor = 0.3f;
        [Range(0.01f, 0.99f)]
        [Tooltip("Player friction against wall")]
        public float frictionAgainstWall = 0.839f;
        [Space(10)]

        [Tooltip("Player can long jump")]
        public bool canLongJump = true;


        [Header("Slope and step specifics")]
        [Tooltip("Distance from the player feet used to check if the player is touching the ground")]
        public float groundCheckerThrashold = 0.1f;
        [Tooltip("Distance from the player feet used to check if the player is touching a slope")]
        public float slopeCheckerThrashold = 0.51f;
        [Tooltip("Distance from the player center used to check if the player is touching a step")]
        public float stepCheckerThrashold = 0.6f;
        [Space(10)]

        [Range(1f, 89f)]
        [Tooltip("Max climbable slope angle")]
        public float maxClimbableSlopeAngle = 53.6f;
        [Tooltip("Max climbable step height")]
        public float maxStepHeight = 0.74f;
        [Space(10)]

        [Tooltip("Speed multiplier based on slope angle")]
        public AnimationCurve speedMultiplierOnAngle = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Range(0.01f, 1f)]
        [Tooltip("Multipler factor on climbable slope")]
        public float canSlideMultiplierCurve = 0.061f;
        [Range(0.01f, 1f)]
        [Tooltip("Multipler factor on non climbable slope")]
        public float cantSlideMultiplierCurve = 0.039f;
        [Range(0.01f, 1f)]
        [Tooltip("Multipler factor on step")]
        public float climbingStairsMultiplierCurve = 0.637f;
        [Space(10)]

        [Tooltip("Multipler factor for gravity")]
        public float gravityMultiplier = 6f;
        [Tooltip("Multipler factor for gravity used on change of normal")]
        public float gravityMultiplyerOnSlideChange = 3f;
        [Tooltip("Multipler factor for gravity used on non climbable slope")]
        public float gravityMultiplierIfUnclimbableSlope = 30f;
        [Space(10)]

        public bool lockOnSlope = false;


        [Header("Wall slide specifics")]
        [Tooltip("Distance from the player head used to check if the player is touching a wall")]
        public float wallCheckerThrashold = 0.8f;
        [Tooltip("Wall checker distance from the player center")]
        public float hightWallCheckerChecker = 0.5f;
        [Space(10)]

        [Tooltip("Multiplier used when the player is jumping from a wall")]
        public float jumpFromWallMultiplier = 30f;
        [Tooltip("Factor used to determine the height of the jump")]
        public float multiplierVerticalLeap = 1f;


        [Header("Sprint and crouch specifics")]
        [Tooltip("Sprint speed")]
        public float sprintSpeed = 20f;
        [Tooltip("Multipler applied to the collider when player is crouching")]
        public float crouchHeightMultiplier = 0.5f;
        [Tooltip("FP camera head height")]
        public Vector3 POV_normalHeadHeight = new Vector3(0f, 0.5f, -0.1f);
        [Tooltip("FP camera head height when crouching")]
        public Vector3 POV_crouchHeadHeight = new Vector3(0f, -0.1f, -0.1f);


        [Header("References")]
        [Tooltip("Character camera")]
        public GameObject characterCamera;
        [Tooltip("Character model")]
        public GameObject characterModel;
        [Tooltip("Character rotation speed when the forward direction is changed")]
        public float characterModelRotationSmooth = 0.1f;
        [Space(10)]

        [Tooltip("Default character mesh")]
        public GameObject meshCharacter;
        [Tooltip("Crouch character mesh")]
        public GameObject meshCharacterCrouch;
        [Tooltip("Head reference")]
        public Transform headPoint;
        [Space(10)]

        // Старый одиночный InputReader удаляем из сетевой логики, 
        // но оставляем переменную как скрытую, чтобы не ломать зависимости, если они есть
        [HideInInspector] public GameObject input;
        [Space(10)]

        public bool debug = true;


        [Header("Events")]
        [SerializeField] UnityEvent OnJump;
        [Space(15)]

        public float minimumVerticalSpeedToLandEvent;
        [SerializeField] UnityEvent OnLand;
        [Space(15)]

        public float minimumHorizontalSpeedToFastEvent;
        [SerializeField] UnityEvent OnFast;
        [Space(15)]

        [SerializeField] UnityEvent OnWallSlide;
        [Space(15)]

        [SerializeField] UnityEvent OnSprint;
        [Space(15)]

        [SerializeField] UnityEvent OnCrouch;
        [Space(15)]



        private Vector3 forward;
        private Vector3 globalForward;
        private Vector3 reactionForward;
        private Vector3 down;
        private Vector3 globalDown;
        private Vector3 reactionGlobalDown;

        private float currentSurfaceAngle;
        private bool currentLockOnSlope;

        private Vector3 wallNormal;
        private Vector3 groundNormal;
        private Vector3 prevGroundNormal;
        private bool prevGrounded;

        private float coyoteJumpMultiplier = 1f;

        public bool isGrounded = false;
        private bool isTouchingSlope = false;
        private bool isTouchingStep = false;
        private bool isTouchingWall = false;
        private bool isJumping = false;
        private bool isCrouch = false;

        private Vector2 axisInput;
        private bool jump;
        private bool jumpHold;
        private bool sprint;
        private bool crouch;

        [HideInInspector]
        public float targetAngle;
        private Rigidbody rigidbody;
        private CapsuleCollider collider;
        private float originalColliderHeight;

        private Vector3 currVelocity = Vector3.zero;
        private float turnSmoothVelocity;
        private bool lockRotation = false;
        private bool lockToCamera = false;

        // Вектор сглаженного направления относительно сетевой камеры
        private Vector3 _networkCameraDirection = Vector3.zero;
        public bool netDashAnimationFlag;

        private void Awake()
        {
            rigidbody = this.GetComponent<Rigidbody>();
            collider = this.GetComponent<CapsuleCollider>();
            originalColliderHeight = collider.height;

            SetFriction(frictionAgainstFloor, true);
            currentLockOnSlope = lockOnSlope;
        }

        // Вместо Start во Fusion используется метод Spawned()
        public override void Spawned()
        {
            // Автоматически находим главную камеру сцены при спавне сетевого тела
            if (characterCamera == null)
            {
                characterCamera = Camera.main != null ? Camera.main.gameObject : null;
            }
        }

        // Обычный Update блокируем для сетевых расчетов — он нам больше не нужен!
        private void Update()
        {
            // Пусто. Сбор ввода выполняет наш InputHandler.cs
        }

        // Обычный FixedUpdate от Unity ПОЛНОСТЬЮ ЗАМЕНЯЕМ на FixedUpdateNetwork от Photon!
        // Этот метод автоматически и синхронно крутится на сервере и клиентах
        public override void FixedUpdateNetwork()
        {
            // ЖЕЛЕЗОБЕТОННЫЙ ПЕРЕХВАТ СЕТЕВОГО ВВОДА FUSION 2.1
            if (GetInput(out NetworkInputData data))
            {
                // 1. Распаковываем WASD и кнопки из нашего сетевого пакета кадра
                axisInput = data.MoveDirection;
                jump = data.JumpPressed;
                jumpHold = data.JumpPressed; // для длинного прыжка дублируем
                sprint = data.DashPressed;
                crouch = false; // Кроуч в Fall Guys не нужен, глушим в false
                
                // 2. МАТЕМАТИЧЕСКИЙ РАСЧЕТ НАПРАВЛЕНИЯ WASD ОТНОСИТЕЛЬНО СЕТЕВОЙ КАМЕРЫ КЛИЕНТА
                if (axisInput.magnitude > movementThrashold)
                {
                    // Восстанавливаем угол камеры, прилетевший по интернету
                    Quaternion cameraYRotation = Quaternion.Euler(0f, data.CameraRotationY, 0f);
                    Vector3 camForward = cameraYRotation * Vector3.forward;
                    Vector3 camRight = cameraYRotation * Vector3.right;

                    // Формируем честный вектор направления движения
                    _networkCameraDirection = (camForward * axisInput.y + camRight * axisInput.x).normalized;

                    // Обновляем целевой угол для вращения модели коровы
                    targetAngle = Mathf.Atan2(axisInput.x, axisInput.y) * Mathf.Rad2Deg + data.CameraRotationY;
                }
                else
                {
                    _networkCameraDirection = Vector3.zero;
                }

                // ====================================================================
                // 3. ЗАПУСК РОДНОЙ ФИЗИЧЕСКОЙ СИСТЕМЫ АССЕТА NAPPIN ВНУТРИ СЕТЕВОГО ТИКА
                // ====================================================================
                CheckGrounded();
                CheckStep();
                CheckWall();
                CheckSlopeAndDirections();

                // Движение ползком
                MoveCrouch();

                // Движение шагом (модифицированный метод, использующий сетевую камеру)
                MoveWalkNetwork();

                // Поворот модели
                if (!lockToCamera) MoveRotation();
                else ForceRotation();

                // Физика прыжка
                MoveJump();

                // Физика гравитации и трения о стены
                ApplyGravity();

                // Вызов сетевых ивентов звуков/частиц
                UpdateEvents();
            }
        }

        // Модифицированный метод ходьбы под сетевые координаты камеры
        private void MoveWalkNetwork()
        {
            float crouchMultiplier = 1f;
            if (isCrouch) crouchMultiplier = crouchSpeedMultiplier;

            if (axisInput.magnitude > movementThrashold)
            {
                // Вместо оригинальной строки Nappin, использовавшей локальный forward,
                // мы толкаем Rigidbody по нашему сетевому вектору _networkCameraDirection!
                Vector3 targetVelocity = _networkCameraDirection * (sprint ? sprintSpeed : movementSpeed) * crouchMultiplier;

                // Сохраняем вертикальную скорость падения/прыжка, чтобы AddForce не ломал гравитацию
                targetVelocity.y = rigidbody.linearVelocity.y;

                rigidbody.linearVelocity = Vector3.SmoothDamp(rigidbody.linearVelocity, targetVelocity, ref currVelocity, dampSpeedUp);
            }
            else
            {
                Vector3 targetVelocity = Vector3.zero;
                targetVelocity.y = rigidbody.linearVelocity.y;
                rigidbody.linearVelocity = Vector3.SmoothDamp(rigidbody.linearVelocity, targetVelocity, ref currVelocity, dampSpeedDown);
            }
        }

        #region Original Checks (Без изменений)

        private void CheckGrounded()
        {
            prevGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, originalColliderHeight / 2f, 0), groundCheckerThrashold, groundMask);
        }

        private void CheckStep()
        {
            bool tmpStep = false;
            Vector3 bottomStepPos = transform.position - new Vector3(0f, originalColliderHeight / 2f, 0f) + new Vector3(0f, 0.05f, 0f);

            RaycastHit stepLowerHit;
            if (Physics.Raycast(bottomStepPos, globalForward, out stepLowerHit, stepCheckerThrashold, groundMask))
            {
                RaycastHit stepUpperHit;
                if (RoundValue(stepLowerHit.normal.y) == 0 &&
                !Physics.Raycast(bottomStepPos + new Vector3(0f, maxStepHeight, 0f), globalForward, out stepUpperHit, stepCheckerThrashold + 0.05f, groundMask))
                {
                    tmpStep = true;
                }
            }

            RaycastHit stepLowerHit45;
            if (Physics.Raycast(bottomStepPos, Quaternion.AngleAxis(45, transform.up) * globalForward, out stepLowerHit45, stepCheckerThrashold, groundMask))
            {
                RaycastHit stepUpperHit45;
                if (RoundValue(stepLowerHit45.normal.y) == 0 && !Physics.Raycast(bottomStepPos + new Vector3(0f, maxStepHeight, 0f), Quaternion.AngleAxis(45, Vector3.up) * globalForward, out stepUpperHit45, stepCheckerThrashold + 0.05f, groundMask))
                {
                    tmpStep = true;
                }
            }

            RaycastHit stepLowerHitMinus45;
            if (Physics.Raycast(bottomStepPos, Quaternion.AngleAxis(-45, transform.up) * globalForward, out stepLowerHitMinus45, stepCheckerThrashold, groundMask))
            {
                RaycastHit stepUpperHitMinus45;
                if (RoundValue(stepLowerHitMinus45.normal.y) == 0 && !Physics.Raycast(bottomStepPos + new Vector3(0f, maxStepHeight, 0f), Quaternion.AngleAxis(-45, Vector3.up) * globalForward, out stepUpperHitMinus45, stepCheckerThrashold + 0.05f, groundMask))
                {
                    tmpStep = true;
                }
            }

            isTouchingStep = tmpStep;
        }

        private void CheckWall()
        {
            bool tmpWall = false;
            Vector3 tmpWallNormal = Vector3.zero;
            Vector3 topWallPos = new Vector3(transform.position.x, transform.position.y + hightWallCheckerChecker, transform.position.z);

            RaycastHit wallHit;
            if (Physics.Raycast(topWallPos, globalForward, out wallHit, wallCheckerThrashold, groundMask)) { tmpWallNormal = wallHit.normal; tmpWall = true; }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(45, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask)) { tmpWallNormal = wallHit.normal; tmpWall = true; }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(90, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask)) { tmpWallNormal = wallHit.normal; tmpWall = true; }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(135, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask)) { tmpWallNormal = wallHit.normal; tmpWall = true; }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(180, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask)) { tmpWallNormal = wallHit.normal; tmpWall = true; }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(225, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask)) { tmpWallNormal = wallHit.normal; tmpWall = true; }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(270, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask)) { tmpWallNormal = wallHit.normal; tmpWall = true; }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(315, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask)) { tmpWallNormal = wallHit.normal; tmpWall = true; }

            isTouchingWall = tmpWall;
            wallNormal = tmpWallNormal;
        }

        private void CheckSlopeAndDirections()
        {
            prevGroundNormal = groundNormal;

            RaycastHit slopeHit;
            if (Physics.SphereCast(transform.position, slopeCheckerThrashold, Vector3.down, out slopeHit, originalColliderHeight / 2f + 0.5f, groundMask))
            {
                groundNormal = slopeHit.normal;

                if (slopeHit.normal.y == 1)
                {
                    forward = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    globalForward = forward;
                    reactionForward = forward;

                    SetFriction(frictionAgainstFloor, true);
                    currentLockOnSlope = lockOnSlope;

                    currentSurfaceAngle = 0f;
                    isTouchingSlope = false;
                }
                else
                {
                    Vector3 tmpGlobalForward = transform.forward.normalized;
                    Vector3 tmpForward = new Vector3(tmpGlobalForward.x, Vector3.ProjectOnPlane(transform.forward.normalized, slopeHit.normal).normalized.y, tmpGlobalForward.z);
                    Vector3 tmpReactionForward = new Vector3(tmpForward.x, tmpGlobalForward.y - tmpForward.y, tmpForward.z);

                    if (currentSurfaceAngle <= maxClimbableSlopeAngle && !isTouchingStep)
                    {
                        forward = tmpForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * canSlideMultiplierCurve) + 1f);
                        globalForward = tmpGlobalForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * canSlideMultiplierCurve) + 1f);
                        reactionForward = tmpReactionForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * canSlideMultiplierCurve) + 1f);

                        SetFriction(frictionAgainstFloor, true);
                        currentLockOnSlope = lockOnSlope;
                    }
                    else if (isTouchingStep)
                    {
                        forward = tmpForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * climbingStairsMultiplierCurve) + 1f);
                        globalForward = tmpGlobalForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * climbingStairsMultiplierCurve) + 1f);
                        reactionForward = tmpReactionForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * climbingStairsMultiplierCurve) + 1f);

                        SetFriction(frictionAgainstFloor, true);
                        currentLockOnSlope = true;
                    }
                    else
                    {
                        forward = tmpForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * cantSlideMultiplierCurve) + 1f);
                        globalForward = tmpGlobalForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * cantSlideMultiplierCurve) + 1f);
                        reactionForward = tmpReactionForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * cantSlideMultiplierCurve) + 1f);

                        SetFriction(0f, true);
                        currentLockOnSlope = lockOnSlope;
                    }

                    currentSurfaceAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                    isTouchingSlope = true;
                }

                down = Vector3.Project(Vector3.down, slopeHit.normal);
                globalDown = Vector3.down.normalized;
                reactionGlobalDown = Vector3.up.normalized;
            }
            else
            {
                groundNormal = Vector3.zero;

                forward = Vector3.ProjectOnPlane(transform.forward, slopeHit.normal).normalized;
                globalForward = forward;
                reactionForward = forward;

                down = Vector3.down.normalized;
                globalDown = Vector3.down.normalized;
                reactionGlobalDown = Vector3.up.normalized;

                SetFriction(frictionAgainstFloor, true);
                currentLockOnSlope = lockOnSlope;
            }
        }

        #endregion

        #region Original Movement core (Без изменений)

        private void MoveCrouch()
        {
            if (crouch && isGrounded)
            {
                isCrouch = true;
                if (meshCharacterCrouch != null && meshCharacter != null) meshCharacter.SetActive(false);
                if (meshCharacterCrouch != null) meshCharacterCrouch.SetActive(true);

                float newHeight = originalColliderHeight * crouchHeightMultiplier;
                collider.height = newHeight;
                collider.center = new Vector3(0f, -newHeight * crouchHeightMultiplier, 0f);

                headPoint.position = new Vector3(transform.position.x + POV_crouchHeadHeight.x, transform.position.y + POV_crouchHeadHeight.y, transform.position.z + POV_crouchHeadHeight.z);
            }
            else
            {
                isCrouch = false;
                if (meshCharacterCrouch != null && meshCharacter != null) meshCharacter.SetActive(true);
                if (meshCharacterCrouch != null) meshCharacterCrouch.SetActive(false);

                collider.height = originalColliderHeight;
                collider.center = Vector3.zero;

                if (headPoint != null)
                    headPoint.position = new Vector3(transform.position.x + POV_normalHeadHeight.x, transform.position.y + POV_normalHeadHeight.y, transform.position.z + POV_normalHeadHeight.z);
            }
        }

        // Оригинальный метод ходьбы Nappin оставляем пустым, так как заменили его на MoveWalkNetwork() выше
        private void MoveWalk() { }

        private void MoveRotation()
        {
            float angle = Mathf.SmoothDampAngle(characterModel.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, characterModelRotationSmooth);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            if (!lockRotation) characterModel.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            else
            {
                var lookPos = -wallNormal;
                lookPos.y = 0;
                var rotation = Quaternion.LookRotation(lookPos);
                characterModel.transform.rotation = rotation;
            }
        }

        public void ForceRotation()
        {
            characterModel.transform.rotation = Quaternion.Euler(0f, characterCamera.transform.rotation.eulerAngles.y, 0f);
        }

        private void MoveJump()
        {
            if (jump && isGrounded && ((isTouchingSlope && currentSurfaceAngle <= maxClimbableSlopeAngle) || !isTouchingSlope) && !isTouchingWall)
            {
                rigidbody.linearVelocity += Vector3.up * jumpVelocity;
                isJumping = true;
            }
            else if (jump && !isGrounded && isTouchingWall)
            {
                rigidbody.linearVelocity += wallNormal * jumpFromWallMultiplier + (Vector3.up * jumpFromWallMultiplier) * multiplierVerticalLeap;
                isJumping = true;

                targetAngle = Mathf.Atan2(wallNormal.x, wallNormal.z) * Mathf.Rad2Deg;

                forward = wallNormal;
                globalForward = forward;
                reactionForward = forward;
            }

            if (rigidbody.linearVelocity.y < 0 && !isGrounded) coyoteJumpMultiplier = fallMultiplier;
            else if (rigidbody.linearVelocity.y > 0.1f && (currentSurfaceAngle <= maxClimbableSlopeAngle || isTouchingStep))
            {
                if (!jumpHold || !canLongJump) coyoteJumpMultiplier = 1f;
                else coyoteJumpMultiplier = 1f / holdJumpMultiplier;
            }
            else
            {
                isJumping = false;
                coyoteJumpMultiplier = 1f;
            }
        }

        #endregion

        #region Gravity and Events (Без изменений)

        private void ApplyGravity()
        {
            Vector3 gravity = Vector3.zero;

            if (currentLockOnSlope || isTouchingStep) gravity = down * gravityMultiplier * -Physics.gravity.y * coyoteJumpMultiplier;
            else gravity = globalDown * gravityMultiplier * -Physics.gravity.y * coyoteJumpMultiplier;

            if (groundNormal.y != 1 && groundNormal.y != 0 && isTouchingSlope && prevGroundNormal != groundNormal)
            {
                gravity *= gravityMultiplyerOnSlideChange;
            }

            if (groundNormal.y != 1 && groundNormal.y != 0 && (currentSurfaceAngle > maxClimbableSlopeAngle && !isTouchingStep))
            {
                if (currentSurfaceAngle > 0f && currentSurfaceAngle <= 30f) gravity = globalDown * gravityMultiplierIfUnclimbableSlope * -Physics.gravity.y;
                else if (currentSurfaceAngle > 30f && currentSurfaceAngle <= 89f) gravity = globalDown * gravityMultiplierIfUnclimbableSlope / 2f * -Physics.gravity.y;
            }

            if (isTouchingWall && rigidbody.linearVelocity.y < 0) gravity *= frictionAgainstWall;

            rigidbody.AddForce(gravity);
        }

        private void UpdateEvents()
        {
            if ((jump && isGrounded && ((isTouchingSlope && currentSurfaceAngle <= maxClimbableSlopeAngle) || !isTouchingSlope)) || (jump && !isGrounded && isTouchingWall)) OnJump.Invoke();
            if (isGrounded && !prevGrounded && rigidbody.linearVelocity.y > -minimumVerticalSpeedToLandEvent) OnLand.Invoke();
            if (Mathf.Abs(rigidbody.linearVelocity.x) + Mathf.Abs(rigidbody.linearVelocity.z) > minimumHorizontalSpeedToFastEvent) OnFast.Invoke();
            if (isTouchingWall && rigidbody.linearVelocity.y < 0) OnWallSlide.Invoke();
            if (sprint) OnSprint.Invoke();
            if (isCrouch) OnCrouch.Invoke();
        }

        #endregion

        #region Friction and Round Tools (Без изменений)

        private void SetFriction(float _frictionWall, bool _isMinimum)
        {
            if (collider == null || collider.material == null) return;
            collider.material.dynamicFriction = 0.6f * _frictionWall;
            collider.material.staticFriction = 0.6f * _frictionWall;

            if (_isMinimum) collider.material.frictionCombine = PhysicsMaterialCombine.Minimum;
            else collider.material.frictionCombine = PhysicsMaterialCombine.Maximum;
        }

        private float RoundValue(float _value)
        {
            float unit = (float)Mathf.Round(_value);
            if (_value - unit < 0.000001f && _value - unit > -0.000001f) return unit;
            else return _value;
        }

        #endregion

        #region GettersSetters (Без изменений)

        public bool GetGrounded() { return isGrounded; }
        public bool GetTouchingSlope() { return isTouchingSlope; }
        public bool GetTouchingStep() { return isTouchingStep; }
        public bool GetTouchingWall() { return isTouchingWall; }
        public bool GetJumping() { return isJumping; }
        public bool GetCrouching() { return isCrouch; }
        public float GetOriginalColliderHeight() { return originalColliderHeight; }

        public void SetLockRotation(bool _lock) { lockRotation = _lock; }
        public void SetLockToCamera(bool _lockToCamera) { lockToCamera = _lockToCamera; if (!_lockToCamera) targetAngle = characterModel.transform.eulerAngles.y; }

        #endregion
    }
}