using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using UnityEngine;

#region Add to custom config

[Serializable]
public class MovementConfig
{
    public bool overwriteSceneMovementType = false;
    public bool overwriteOtherSceneMovementSettings = false;

    public MovementController.MovementConfig movementSettings = new MovementController.MovementConfig();
}
public partial class CustomAppConfig
{
    public MovementConfig movement = new MovementConfig();
}

#endregion


/**
 *    Movement mechanics based on Unity Standard Assets' first person characters
**/

[HelpURL(Lzwp.LzwpLibManualUrl + "#MovementController")]
[RequireComponent(typeof(Rigidbody), typeof(CharacterController), typeof(CapsuleCollider))]
public class MovementController : MonoBehaviour
{
    #region Config

    public enum MovementType
    {
        None,
        Flying,
        Walking
    }

    [Serializable]
    public class GeneralConfig
    {
        [Serializable]
        public class GeneralKeyboardAndMouseConfig
        {
            [JsonConverter(typeof(StringEnumConverter))] public KeyCode moveFasterKey = KeyCode.LeftShift;
            [JsonConverter(typeof(StringEnumConverter))] public KeyCode moveFasterKeyAlt = KeyCode.None;
            [JsonConverter(typeof(StringEnumConverter))] public KeyCode moveSlowerKey = KeyCode.LeftControl;
            [JsonConverter(typeof(StringEnumConverter))] public KeyCode moveSlowerKeyAlt = KeyCode.RightShift;

            public float mouseRotationSpeed = 2;
        }

        [Serializable]
        public class GeneralFlystickConfig
        {
            public enum JoystickHorizontalAction
            {
                Strafe,
                Rotate
            }

            public bool anyFlystick = true;
            public int flystickIndex = 0;

            [Space]

            [JsonConverter(typeof(StringEnumConverter))]
            public JoystickHorizontalAction joystickHorizontalAction = JoystickHorizontalAction.Rotate;

            public float rotateSpeed = 0.5f;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public MovementType movementType = MovementType.Flying;
        public bool dblJumpToToggleMovementType = true;
        public float dblJumpDetectionTime = 0.5f;

        public float rotationSmoothSpeed = 10f;

        public GeneralKeyboardAndMouseConfig keyboardAndMouse = new GeneralKeyboardAndMouseConfig();
        public GeneralFlystickConfig flystick = new GeneralFlystickConfig();
    }

    [Serializable]
    public class FlyingConfig
    {
        [Serializable]
        public class FlyingKeyboardAndMouseConfig
        {
            [JsonConverter(typeof(StringEnumConverter))] public KeyCode flyUpKey = KeyCode.E;
            [JsonConverter(typeof(StringEnumConverter))] public KeyCode flyUpKeyAlt = KeyCode.Space;
            [JsonConverter(typeof(StringEnumConverter))] public KeyCode flyDownKey = KeyCode.Q;

            public float NormalMoveSpeed = 7;
            public float MoveSlowerMultiplier = 0.25f;
            public float MoveFasterMultiplier = 4;
        }

        [Serializable]
        public class FlyingFlystickConfig
        {
            public float moveSpeed = 7f;
        }

        public bool useCapsuleCollider = false;

        public FlyingKeyboardAndMouseConfig keyboardAndMouse = new FlyingKeyboardAndMouseConfig();
        public FlyingFlystickConfig flystick = new FlyingFlystickConfig();
    }

    [Serializable]
    public class WalkingConfig
    {
        public enum WalkingType
        {
            CharacterController,
            PhysicsBased
        }

        [Serializable]
        public class CharacterControllerWalkingTypeConfig
        {
            public float walkSpeed = 5f;
            public float jumpSpeed = 5f;
            public float stickToGroundForce = 10f;
            public float gravityMultiplier = 1f;
        }

        [Serializable]
        public class PhysicsBasedWalkingTypeConfig
        {
            public float walkSpeed = 5f;

            public float JumpForce = 65f;
            [JsonConverter(typeof(LzwpUtils.Converters.AnimationCurveConverter))]
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));

            public float groundCheckDistance = 0.1f;
            public float stickToGroundHelperDistance = 0.6f;
            public bool airControl;
        }

        [Serializable]
        public class WalkingFlystickConfig
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public LzwpInput.Flystick.ButtonID jumpBtn = LzwpInput.Flystick.ButtonID.Fire;

            public bool walkInFlystickDirection = true;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public WalkingType walkingType = WalkingType.CharacterController;
        public bool jumpingEnabled = true;

        public WalkingFlystickConfig flystick = new WalkingFlystickConfig();

        [Space]
        public float runSpeedMultiplier = 2f;
        public float strafeSpeedMultiplier = 0.9f;
        public float backwardSpeedMultiplier = 0.6f;
        public float stepInterval = 5f;
        [Range(0f, 1f)] public float runstepLenghten = 0.7f;

        [Space]

        public bool playWalkingAudio = true;
        public bool playJumpingAudio = true;
        [JsonIgnore] public AudioClip[] footstepSounds;
        [JsonIgnore] public AudioClip jumpSound;
        [JsonIgnore] public AudioClip landSound;

        [Space]

        public CharacterControllerWalkingTypeConfig characterControllerWalkingType = new CharacterControllerWalkingTypeConfig();
        public PhysicsBasedWalkingTypeConfig physicsBasedWalkingType = new PhysicsBasedWalkingTypeConfig();
    }

    [Serializable]
    public class MovementConfig
    {
        public GeneralConfig general = new GeneralConfig();
        public FlyingConfig flying = new FlyingConfig();
        public WalkingConfig walking = new WalkingConfig();
    }

    public MovementConfig config;

    #endregion
    
    // rotation
    float rotationY = 0.0f;
    float rotationX = 0.0f;

    // walking common
    bool jump = false;
    bool isJumping = false;
    bool previouslyGrounded = false;
    bool isGrounded = false;
    bool isRunning = false;

    [HideInInspector] public float currentSpeed = 8f;

    float walkingStepCycle = 0;
    float walkingNextStep = 0;

    // walking - character controller
    Vector3 walkingCC_moveDir = Vector3.zero;
    CollisionFlags walkingCC_collisionFlags;

    // walking - physics based
    Vector3 walkingP_groundContactNormal;

    float lastJumpPressedTime = 0;



    CharacterController characterController;
    Rigidbody rb;
    CapsuleCollider capsuleCollider;
    AudioSource audioSource;

    bool started = false;

    Action<LzwpInput.Flystick.ButtonID>[] flystickBtnPressAction = { };


    #region Some properties

    public bool Grounded
    {
        get { return config.general.movementType == MovementType.Walking && isGrounded; }
    }

    public bool Jumping
    {
        get { return config.general.movementType == MovementType.Walking && isJumping; }
    }

    public bool Running
    {
        get { return config.general.movementType == MovementType.Walking && isRunning; }
    }

    public Vector3 Velocity
    {
        get
        {
            if (config.general.movementType == MovementType.Flying)
            {
                // TBD
            }
            else if (config.general.movementType == MovementType.Walking)
            {
                if (config.walking.walkingType == WalkingConfig.WalkingType.CharacterController)
                    return characterController.velocity;
                else if (config.walking.walkingType == WalkingConfig.WalkingType.PhysicsBased)
                    return rb.velocity;
            }

            return Vector3.zero;
        }
    }

    #endregion

    private void OnEnable()
    {
        Lzwp.AddAfterInitializedAction(InitMovementController);
        UpdateTargetRotation();
    }

    private void OnDisable()
    {
        if (Lzwp.Instance != null)
        {
            Lzwp.RemoveAfterInitializedAction(InitMovementController);

            if (Lzwp.initialized)
            {
                for (int i = 0; i < Lzwp.input.flysticks.Count && i < flystickBtnPressAction.Length; i++)
                {
                    Lzwp.input.flysticks[i].OnButtonPress -= flystickBtnPressAction[i];
                }
            }
        }
    }

    void InitMovementController()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        audioSource = GetComponent<AudioSource>();

        if (!Lzwp.sync.isMaster)
        {
            DestroyImmediate(this);

            try
            {
                if (rb != null) DestroyImmediate(rb);
            }
            catch (Exception) { }

            try
            {
                if (characterController != null) DestroyImmediate(characterController);
            }
            catch (Exception) { }

            try
            {
                if (capsuleCollider != null) DestroyImmediate(capsuleCollider);
            }
            catch (Exception) { }

            try
            {
                if (audioSource != null) DestroyImmediate(audioSource);
            }
            catch (Exception) { }

            return;
        }



        if (Lzwp.config.GetCustom().movement.overwriteSceneMovementType)
            config.general.movementType = Lzwp.config.GetCustom().movement.movementSettings.general.movementType;

        MovementType _movementType = config.general.movementType;

        if (Lzwp.config.GetCustom().movement.overwriteOtherSceneMovementSettings)
        {
            config = Lzwp.config.GetCustom().movement.movementSettings;
            config.general.movementType = _movementType;
        }

        started = true;
        MovementTypeChanged();

        flystickBtnPressAction = new Action<LzwpInput.Flystick.ButtonID>[Lzwp.input.flysticks.Count];

        for (int i = 0; i < Lzwp.input.flysticks.Count; i++)
        {
            int flystickIdx = i;
            //Lzwp.input.flysticks[i].OnButtonPress += (btn) =>
            //{
            //    FlystickButtonPress(btn, flystickIdx);
            //};

            flystickBtnPressAction[i] = (btn) =>
            {
                FlystickButtonPress(btn, flystickIdx);
            };


            Lzwp.input.flysticks[i].OnButtonPress += flystickBtnPressAction[i];
        }
    }
    

    void OnValidate()
    {
        if (Application.isPlaying)
            MovementTypeChanged();
    }

    void ToggleMovementType()
    {
        if (config.general.movementType == MovementType.Flying)
            SetMovementType(MovementType.Walking);
        else if (config.general.movementType == MovementType.Walking)
            SetMovementType(MovementType.Flying);
    }

    public void SetMovementType(MovementType mt)
    {
        config.general.movementType = mt;
        MovementTypeChanged();
    }

    public void SetWalkingType(WalkingConfig.WalkingType wt)
    {
        config.walking.walkingType = wt;
        MovementTypeChanged();
    }

    void MovementTypeChanged()
    {
        if (!started)
            return;

        capsuleCollider.enabled = ((config.general.movementType == MovementType.Walking && config.walking.walkingType == WalkingConfig.WalkingType.PhysicsBased) || (config.flying.useCapsuleCollider && config.general.movementType == MovementType.Flying));
        rb.isKinematic = !(config.general.movementType == MovementType.Walking && config.walking.walkingType == WalkingConfig.WalkingType.PhysicsBased);
        characterController.enabled = (config.general.movementType == MovementType.Walking && config.walking.walkingType == WalkingConfig.WalkingType.CharacterController);

        if (config.general.movementType == MovementType.Walking)
        {
            rotationX = 0;
            transform.localRotation = Quaternion.AngleAxis(rotationY, Vector3.up) * Quaternion.AngleAxis(rotationX, Vector3.right);
        }
    }

    void Reset()
    {
        if (Application.isEditor)
        {
            if (config.walking.footstepSounds == null || config.walking.footstepSounds.Length == 0)
            {
                List<AudioClip> footstepClips = new List<AudioClip>();
                for (int i = 1; i <= 4; i++)
                {
                    var cl = LzwpUtils.LoadAssetAtPath<AudioClip>(string.Format("Assets/LZWPlib/Audio/Footstep0{0}.wav", i));
                    if (cl != null)
                        footstepClips.Add(cl);
                }

                config.walking.footstepSounds = footstepClips.ToArray();
            }

            if (config.walking.jumpSound == null)
                config.walking.jumpSound = LzwpUtils.LoadAssetAtPath<AudioClip>("Assets/LZWPlib/Audio/Jump.wav");

            if (config.walking.landSound == null)
                config.walking.landSound = LzwpUtils.LoadAssetAtPath<AudioClip>("Assets/LZWPlib/Audio/Land.wav");
        }
    }

    void Update()
    {
        if (config.general.movementType == MovementType.Flying)
            FlyingUpdate();
        else if (config.general.movementType == MovementType.Walking)
        {
            if (config.walking.walkingType == WalkingConfig.WalkingType.CharacterController)
                WalkingCC_Update();
            else
                WalkingPhysics_Update();
        }

        if (Input.GetButtonDown("Jump"))
            CheckDblJump();

        UpdateViewRotation();
    }

    void FixedUpdate()
    {
        if (config.general.movementType == MovementType.Walking)
        {
            if (config.walking.walkingType == WalkingConfig.WalkingType.CharacterController)
                WalkingCC_FixedUpdate();
            else
                WalkingPhysics_FixedUpdate();
        }
    }


    #region Input

    Vector2 GetFlystickJoystickInput()
    {
        if (Lzwp.initialized)
        {
            if (config.general.flystick.anyFlystick)
            {
                Vector2 v = Vector2.zero;
                foreach (var fl in Lzwp.input.flysticks)
                {
                    v.x += fl.joysticks[0];
                    v.y += fl.joysticks[1];
                }

                return v;
            }
            else if (Lzwp.input.flysticks.Count > config.general.flystick.flystickIndex && config.general.flystick.flystickIndex >= 0)
                return new Vector2(Lzwp.input.flysticks[config.general.flystick.flystickIndex].joysticks[0], Lzwp.input.flysticks[config.general.flystick.flystickIndex].joysticks[1]);
        }

        return Vector2.zero;
    }

    Vector3 GetFlystickDirectedMoveVector(float strafeMultiplier = 1f, float backwardMultiplier = 1f)
    {
        if (Lzwp.initialized)
        {
            if (config.general.flystick.anyFlystick)
            {
                Vector3 v = Vector3.zero;

                foreach (var fl in Lzwp.input.flysticks)
                {
                    v += fl.pose.rotation * Vector3.forward * fl.joysticks[1] * (fl.joysticks[1] < 0 ? backwardMultiplier : 1f);

                    if (config.general.flystick.joystickHorizontalAction == GeneralConfig.GeneralFlystickConfig.JoystickHorizontalAction.Strafe)
                        v += fl.pose.rotation * Vector3.right * fl.joysticks[0] * strafeMultiplier;
                }

                return v;
            }
            else if (Lzwp.input.flysticks.Count > config.general.flystick.flystickIndex && config.general.flystick.flystickIndex >= 0)
            {
                float z = Lzwp.input.flysticks[config.general.flystick.flystickIndex].joysticks[1];

                Vector3 v = Lzwp.input.flysticks[config.general.flystick.flystickIndex].pose.rotation * Vector3.forward * z * (z < 0 ? backwardMultiplier : 1f);

                if (config.general.flystick.joystickHorizontalAction == GeneralConfig.GeneralFlystickConfig.JoystickHorizontalAction.Strafe)
                    v += Lzwp.input.flysticks[config.general.flystick.flystickIndex].pose.rotation * Vector3.right * Lzwp.input.flysticks[config.general.flystick.flystickIndex].joysticks[0] * strafeMultiplier;

                return v;
            }
        }

        return Vector3.zero;
    }

    Vector3 GetKeyboardMoveInput()
    {
        return new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetKey(config.flying.keyboardAndMouse.flyUpKey) || Input.GetKey(config.flying.keyboardAndMouse.flyUpKeyAlt) ? 1f : (Input.GetKey(config.flying.keyboardAndMouse.flyDownKey) ? -1f : 0),
            Input.GetAxis("Vertical")
        );
    }

    Vector3 GetWalkDirection()
    {
        Vector3 res = GetKeyboardMoveInput();

        res.x *= config.walking.strafeSpeedMultiplier;
        if (res.z < 0)
            res.z *= config.walking.backwardSpeedMultiplier;

        res = transform.rotation * res;


        if (config.walking.flystick.walkInFlystickDirection)
        {
            res += GetFlystickDirectedMoveVector(config.walking.strafeSpeedMultiplier, config.walking.backwardSpeedMultiplier);
        }
        else
        {
            Vector2 v = GetFlystickJoystickInput();

            if (v.y < 0)
                v.y *= config.walking.backwardSpeedMultiplier;

            if (config.general.flystick.joystickHorizontalAction == GeneralConfig.GeneralFlystickConfig.JoystickHorizontalAction.Strafe)
                v.x *= config.walking.strafeSpeedMultiplier;

            res += transform.forward * v.y + transform.right * v.x;
        }

        return res;
    }

    void CheckDblJump()
    {
        if (config.general.dblJumpToToggleMovementType)
        {
            if (Time.realtimeSinceStartup - lastJumpPressedTime < config.general.dblJumpDetectionTime)
            {
                ToggleMovementType();
                jump = false;
            }

            lastJumpPressedTime = Time.realtimeSinceStartup;
        }
    }

    void CheckIfWantToJump()
    {
        if (config.walking.jumpingEnabled && !jump && !isJumping)
            jump = Input.GetButtonDown("Jump");
    }

    void FlystickButtonPress(LzwpInput.Flystick.ButtonID btn, int flystickIdx)
    {
        if ((config.general.flystick.anyFlystick || flystickIdx == config.general.flystick.flystickIndex) && btn == config.walking.flystick.jumpBtn)
        {
            if (config.walking.jumpingEnabled && !jump && !isJumping)
                jump = true;

            CheckDblJump();
        }
    }

    #endregion

    #region Rotation

    void RotateView(bool horizontalRotation = true, bool verticalRotation = true)
    {
        // mouse - rotation
        if (Input.GetMouseButton(1))
        {
            if (horizontalRotation)
                rotationY += Input.GetAxis("Mouse X") * config.general.keyboardAndMouse.mouseRotationSpeed;

            if (verticalRotation)
            {
                rotationX -= Input.GetAxis("Mouse Y") * config.general.keyboardAndMouse.mouseRotationSpeed;
                rotationX = Mathf.Clamp(rotationX, -90, 90);
            }
        }

        // flytick - rotation
        if (config.general.flystick.joystickHorizontalAction == GeneralConfig.GeneralFlystickConfig.JoystickHorizontalAction.Rotate)
        {
            Vector2 flInp = GetFlystickJoystickInput();
            rotationY += flInp.x * config.general.flystick.rotateSpeed;
        }
    }


    void UpdateViewRotation()
    {
        LzwpPose rotationPivotPose = null;
        Vector3 oldPivotPointPosition = Vector3.zero;

        if (Lzwp.initialized)
        {
            rotationPivotPose = Lzwp.display.pointsOfView[0].targetPose;
            oldPivotPointPosition = rotationPivotPose.position;
        }

        Quaternion targetRotation = Quaternion.AngleAxis(rotationY, Vector3.up) * Quaternion.AngleAxis(rotationX, Vector3.right);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, config.general.rotationSmoothSpeed * Time.deltaTime);


        if (Lzwp.initialized)
            transform.position += oldPivotPointPosition - rotationPivotPose.position;
    }

    public void UpdateTargetRotation()
    {
        rotationX = transform.localEulerAngles.x;
        rotationY = transform.localEulerAngles.y;
    }

    #endregion

    #region Flying

    void FlyingUpdate()
    {
        RotateView();

        // keyboard - movement

        float speed = config.flying.keyboardAndMouse.NormalMoveSpeed;
        isRunning = Input.GetKey(config.general.keyboardAndMouse.moveFasterKey) || Input.GetKey(config.general.keyboardAndMouse.moveFasterKeyAlt);
        if (isRunning)
            speed *= config.flying.keyboardAndMouse.MoveFasterMultiplier;
        else if (Input.GetKey(config.general.keyboardAndMouse.moveSlowerKey) || Input.GetKey(config.general.keyboardAndMouse.moveSlowerKeyAlt))
            speed *= config.flying.keyboardAndMouse.MoveSlowerMultiplier;

        Vector3 inp = GetKeyboardMoveInput();

        transform.position += transform.rotation * inp * speed * Time.deltaTime;

        // flystick - movement
        transform.position += GetFlystickDirectedMoveVector() * config.flying.flystick.moveSpeed * Time.deltaTime;
    }

    #endregion

    #region Walking

    void ProgressStepCycle(Vector3 input)
    {
        if (Velocity.sqrMagnitude > 0 && (input.x != 0 || input.y != 0 || input.z != 0))
            walkingStepCycle += (Velocity.magnitude + (currentSpeed * (isRunning ? config.walking.runstepLenghten : 1f))) * Time.fixedDeltaTime;

        if (!(walkingStepCycle > walkingNextStep))
            return;

        walkingNextStep = walkingStepCycle + config.walking.stepInterval;

        PlayFootStepAudio();
    }

    #endregion

    #region Walking - Character Controller

    void WalkingCC_Update()
    {
        RotateView(true, false);

        CheckIfWantToJump();

        if (!previouslyGrounded && characterController.isGrounded)
        {
            PlayLandingSound();
            walkingCC_moveDir.y = 0f;
            isJumping = false;
        }

        if (!characterController.isGrounded && !isJumping && previouslyGrounded)
        {
            walkingCC_moveDir.y = 0f;
        }

        previouslyGrounded = characterController.isGrounded;
    }

    void WalkingCC_FixedUpdate()
    {
        isRunning = Input.GetKey(config.general.keyboardAndMouse.moveFasterKey) || Input.GetKey(config.general.keyboardAndMouse.moveFasterKeyAlt);
        currentSpeed = config.walking.characterControllerWalkingType.walkSpeed;
        if (isRunning)
            currentSpeed *= config.walking.runSpeedMultiplier;

        Vector3 desiredMove = GetWalkDirection();

        if (desiredMove.sqrMagnitude > 1)
            desiredMove.Normalize();

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position + characterController.center, characterController.radius, Vector3.down, out hitInfo, characterController.height / 2f);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        walkingCC_moveDir.x = desiredMove.x * currentSpeed;
        walkingCC_moveDir.z = desiredMove.z * currentSpeed;


        if (characterController.isGrounded)
        {
            walkingCC_moveDir.y = -config.walking.characterControllerWalkingType.stickToGroundForce;

            if (jump)
            {
                walkingCC_moveDir.y = config.walking.characterControllerWalkingType.jumpSpeed;
                PlayJumpSound();
                jump = false;
                isJumping = true;
            }
        }
        else
        {
            walkingCC_moveDir += Physics.gravity * config.walking.characterControllerWalkingType.gravityMultiplier * Time.fixedDeltaTime;
        }
        walkingCC_collisionFlags = characterController.Move(walkingCC_moveDir * Time.fixedDeltaTime);

        isGrounded = characterController.isGrounded;


        ProgressStepCycle(desiredMove);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (walkingCC_collisionFlags == CollisionFlags.Below)  // don't move the rigidbody if the character is on top of it
            return;

        if (body == null || body.isKinematic)
            return;

        body.AddForceAtPosition(characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }

    #endregion
    
    #region Walking - Physics based

    void WalkingPhysics_Update()
    {
        float oldYRotation = transform.eulerAngles.y;  // get the rotation before it's changed

        RotateView(true, false);

        if (isGrounded || config.walking.physicsBasedWalkingType.airControl)
        {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            rb.velocity = velRotation * rb.velocity;
        }

        CheckIfWantToJump();
    }

    void WalkingPhysics_FixedUpdate()
    {
        GroundCheck();

        isRunning = Input.GetKey(config.general.keyboardAndMouse.moveFasterKey) || Input.GetKey(config.general.keyboardAndMouse.moveFasterKeyAlt);
        currentSpeed = config.walking.physicsBasedWalkingType.walkSpeed;
        if (isRunning)
            currentSpeed *= config.walking.runSpeedMultiplier;

        Vector3 desiredMove = GetWalkDirection();


        if ((Mathf.Abs(desiredMove.x) > float.Epsilon || Mathf.Abs(desiredMove.y) > float.Epsilon || Mathf.Abs(desiredMove.z) > float.Epsilon) && (config.walking.physicsBasedWalkingType.airControl || isGrounded))
        {
            desiredMove = Vector3.ProjectOnPlane(desiredMove, walkingP_groundContactNormal).normalized;

            desiredMove.x = desiredMove.x * currentSpeed;
            desiredMove.z = desiredMove.z * currentSpeed;
            desiredMove.y = desiredMove.y * currentSpeed;

            if (rb.velocity.sqrMagnitude < (currentSpeed * currentSpeed))
                rb.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
        }

        if (isGrounded)
        {
            rb.drag = 15f;

            if (jump)
            {
                PlayJumpSound();
                rb.drag = 0f;
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(new Vector3(0f, config.walking.physicsBasedWalkingType.JumpForce, 0f), ForceMode.Impulse);
                isJumping = true;
            }

            if (!isJumping && Mathf.Abs(desiredMove.x) < float.Epsilon && Mathf.Abs(desiredMove.y) < float.Epsilon && Mathf.Abs(desiredMove.z) < float.Epsilon && rb.velocity.magnitude < 1f)
                rb.Sleep();
        }
        else
        {
            rb.drag = 0f;
            if (previouslyGrounded && !isJumping)
                StickToGroundHelper();
        }

        jump = false;
        
        ProgressStepCycle(desiredMove);
    }

    private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(walkingP_groundContactNormal, Vector3.up);
        return config.walking.physicsBasedWalkingType.SlopeCurveModifier.Evaluate(angle);
    }


    private void StickToGroundHelper()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position + capsuleCollider.center, capsuleCollider.radius, Vector3.down, out hitInfo, ((capsuleCollider.height / 2f) - capsuleCollider.radius) + config.walking.physicsBasedWalkingType.stickToGroundHelperDistance))
        {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            {
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hitInfo.normal);
            }
        }
    }

    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck()
    {
        previouslyGrounded = isGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position + capsuleCollider.center, capsuleCollider.radius, Vector3.down, out hitInfo, ((capsuleCollider.height / 2f) - capsuleCollider.radius) + config.walking.physicsBasedWalkingType.groundCheckDistance))
        {
            isGrounded = true;
            walkingP_groundContactNormal = hitInfo.normal;
        }
        else
        {
            isGrounded = false;
            walkingP_groundContactNormal = Vector3.up;
        }

        if (!previouslyGrounded && isGrounded && isJumping)
        {
            PlayLandingSound();
            isJumping = false;
        }
    }

    #endregion

    #region Audio

    private void PlayJumpSound()
    {
        if (!config.walking.playJumpingAudio || config.walking.jumpSound == null)
            return;

        audioSource.clip = config.walking.jumpSound;
        audioSource.Play();
    }

    private void PlayLandingSound()
    {
        if (!config.walking.playJumpingAudio || config.walking.landSound == null)
            return;

        audioSource.clip = config.walking.landSound;
        audioSource.Play();
        walkingNextStep = walkingStepCycle + .5f;
    }

    private void PlayFootStepAudio()
    {
        if (!config.walking.playWalkingAudio || !isGrounded || config.walking.footstepSounds.Length < 1)
            return;

        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = UnityEngine.Random.Range(1, config.walking.footstepSounds.Length);
        audioSource.clip = config.walking.footstepSounds[n];
        audioSource.PlayOneShot(audioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        config.walking.footstepSounds[n] = config.walking.footstepSounds[0];
        config.walking.footstepSounds[0] = audioSource.clip;
    }

    #endregion
}