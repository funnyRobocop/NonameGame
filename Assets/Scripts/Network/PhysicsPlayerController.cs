using UnityEngine;
using UnityEngine.Events;
using System;
using Fusion;

namespace NonameGame
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsPlayerController : NetworkBehaviour
    {
        [Header("Movement specifics")]
        [Tooltip("Layers where the player can stand on")]
        [SerializeField] LayerMask groundMask;
        [Tooltip("Base player speed")]
        public float movementSpeed = 14f;
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
        [Range(0f, 1f)]
        [Tooltip("Player friction against floor")]
        public float frictionAgainstFloor = 0.3f;
        [Range(0.01f, 0.99f)]
        [Tooltip("Player friction against wall")]
        public float frictionAgainstWall = 0.839f;
        [Space(10)]

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

        [Header("References")]
        [Tooltip("Character camera")]
        public GameObject characterCamera;
        [Tooltip("Character model")]
        public GameObject characterModel;
        [Tooltip("Character rotation speed when the forward direction is changed")]
        public float characterModelRotationSmooth = 0.1f;
        [Space(10)]

        [Tooltip("Head reference")]
        public Transform headPoint;
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

        private bool isGrounded = false;
        private bool isTouchingSlope = false;
        private bool isTouchingStep = false;
        private bool isTouchingWall = false;
        private bool isJumping = false;
        private bool isCrouch = false;

        private Vector2 axisInput;
        private bool jump;

        [HideInInspector]
        public float targetAngle;
        private Rigidbody rigidbody;
        private CapsuleCollider collider;
        private float originalColliderHeight;

        private Vector3 currVelocity = Vector3.zero;
        private float turnSmoothVelocity;
        private bool lockRotation = false;

        // Вектор сглаженного направления относительно сетевой камеры
        private Vector3 _networkCameraDirection = Vector3.zero;
        public bool netDashAnimationFlag;

        [Header("Сетевой Рывок (Физический Dash)")]
        [Tooltip("Сила импульса рывка вперед")]
        [SerializeField] private float dashForce = 15f;
        [Tooltip("Длительность фазы рывка в секундах, на которую отключается WASD для сочности полета")]
        [SerializeField] private float dashStunDuration = 0.25f;

        [Networked] private NetworkBool _hasDashedInAir { get; set; }
        [Networked] private TickTimer _dashStunTimer { get; set; }
        [Networked] private Vector3 _dashStoredDirection { get; set; }
        [Networked] public TickTimer stunTimer { get; set; }


        private void Awake()
        {
            rigidbody = this.GetComponent<Rigidbody>();
            collider = this.GetComponent<CapsuleCollider>();
            originalColliderHeight = collider.height;

            SetFriction(frictionAgainstFloor, true);
            currentLockOnSlope = lockOnSlope;
        }

        public override void Spawned()
        {
            if (characterCamera == null)
            {
                characterCamera = Camera.main != null ? Camera.main.gameObject : null;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                axisInput = data.MoveDirection;
                jump = data.JumpPressed;
                
                if (isGrounded)
                {
                    _hasDashedInAir = false;
                }       
                
                if (axisInput.magnitude > movementThrashold)
                {
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

                CheckGrounded();
                //CheckStep();
                //CheckWall();
                CheckSlopeAndDirections();

                if (!stunTimer.ExpiredOrNotRunning(Runner))
                {
                    netDashAnimationFlag = true; 
                }
                else if (!_dashStunTimer.ExpiredOrNotRunning(Runner))
                {
                    Vector3 currentVel = rigidbody.linearVelocity;
                    rigidbody.linearVelocity = new Vector3(_dashStoredDirection.x * dashForce, currentVel.y, _dashStoredDirection.z * dashForce);
                }
                else
                {
                    MoveWalkNetwork();
                    MoveRotation();
                    MoveJump();

                    if (data.JumpPressed && !isGrounded && !_hasDashedInAir)
                    {
                        _hasDashedInAir = true;
                        _dashStunTimer = TickTimer.CreateFromSeconds(Runner, dashStunDuration);
                        _dashStoredDirection = (_networkCameraDirection != Vector3.zero) ? _networkCameraDirection : characterModel.transform.forward;
                        rigidbody.linearVelocity = Vector3.zero;
                        Vector3 impulseVector = _dashStoredDirection;
                        impulseVector.y = 0.2f;
                        rigidbody.AddForce(impulseVector.normalized * dashForce, ForceMode.Impulse);

                        TriggerDashAnimation();

                        Debug.Log($"[Физический Рывок] Rigidbody запущен вперед с силой {dashForce}!");
                    }
                }
                
                // Физика гравитации и трения о стены
                ApplyGravity();

                // Вызов сетевых ивентов звуков/частиц
                UpdateEvents();
            }
        }

        private void TriggerDashAnimation()
        {
            netDashAnimationFlag = true;
        }

        private void MoveWalkNetwork()
        {
            if (axisInput.magnitude > movementThrashold)
            {
                Vector3 targetVelocity = _networkCameraDirection * movementSpeed;
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
            else
            {
                isJumping = false;
                coyoteJumpMultiplier = 1f;
            }
        }

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
        }

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

        public bool GetGrounded() { return isGrounded; }
        public bool GetTouchingSlope() { return isTouchingSlope; }
        public bool GetTouchingStep() { return isTouchingStep; }
        public bool GetTouchingWall() { return isTouchingWall; }
        public bool GetJumping() { return isJumping; }
        public bool GetCrouching() { return isCrouch; }
        public float GetOriginalColliderHeight() { return originalColliderHeight; }
    }
}