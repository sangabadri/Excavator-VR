using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Controls;

public class LeverAngles
{
    public float upDown;
    public float leftRight;

    public LeverAngles(float upDown, float leftRight)
    {
        this.upDown = upDown;
        this.leftRight = leftRight;
    }

    public bool isOperated() { return (this.upDown != 0 || this.leftRight != 0); }
    public void clear() { this.upDown = 0F; this.leftRight = 0F; }
}

public class InputEvents
{
    private float _swing, _boom, _arm, _bucket, _trackRight, _trackLeft;
    private bool _updated;

    public void clear()
    {
        _swing = _boom = _arm = _bucket = _trackRight = _trackLeft = 0F;
        _updated = false;
    }

    public bool isUpdated { get { return _updated; } }
    public float swing { set { _swing = value; _updated = true; } get { return _swing; } }
    public float boom { set { _boom = value; _updated = true; } get { return _boom; } }
    public float arm { set { _arm = value; _updated = true; } get { return _arm; } }
    public float bucket { set { _bucket = value; _updated = true; } get { return _bucket; } }
    public float trackRight { set { _trackRight = value; _updated = true; } get { return _trackRight; } }
    public float trackLeft { set { _trackLeft = value; _updated = true; } get { return _trackLeft; } }
}

public class ExcavatorController : MonoBehaviour
{
    [HideInInspector] public bool lockSwing = false;
    [HideInInspector] public bool lockBoom = false;
    [HideInInspector] public bool lockArm = false;
    [HideInInspector] public bool lockBucket = false;

    [Header("Track Physics")]
    public float maxSpeed = 3F;
    public float creepSpeed = 1F;
    public float initialAccel = 8F;
    public float maxAccel = 20F;
    public float deltaAccel = 0.5F;
    public float deltaDirection = 2F;

    [Header("Track Steering")]
    public float trackSteerSpeed = 10F;

    [Header("Swing Joint Physics")]
    public float swingHydraulicForce = 150000f;
    public float swingEffectiveMass = 8000f;
    public float swingLeverArm = 1.5f;
    public float swingDamping = 3f;
    public float swingMaxAngularSpeed = 6f;

    [Header("Boom Joint Physics")]
    public float boomHydraulicForce = 200000f;
    public float boomEffectiveMass = 3500f;
    public float boomLeverArm = 2.0f;
    public float boomDamping = 2.5f;
    public float boomMaxAngularSpeed = 4f;

    [Header("Arm Joint Physics")]
    public float armHydraulicForce = 120000f;
    public float armEffectiveMass = 1500f;
    public float armLeverArm = 1.2f;
    public float armDamping = 3f;
    public float armMaxAngularSpeed = 5f;

    [Header("Bucket Joint Physics")]
    public float bucketHydraulicForce = 80000f;
    public float bucketEffectiveMass = 500f;
    public float bucketLeverArm = 0.4f;
    public float bucketDamping = 4f;
    public float bucketMaxAngularSpeed = 7f;

    [Header("Audio")]
    [Tooltip("AudioSource for engine idle. Assign your idle clip to its AudioClip field.")]
    public AudioSource engineIdleAudio;

    [Tooltip("AudioSource for track/working sound. Assign your moving clip to its AudioClip field.")]
    public AudioSource trackMovingAudio;

    [Tooltip("Volume of engine idle sound (0–1).")]
    [Range(0f, 1f)]
    public float engineIdleVolume = 0.7f;

    [Tooltip("Idle engine pitch — no tracks moving.")]
    public float engineIdlePitch = 1.0f;

    [Tooltip("Engine pitch when tracks are active (under load).")]
    public float engineUnderLoadPitch = 1.15f;

    [Tooltip("Track sound volume with one track active.")]
    [Range(0f, 1f)]
    public float oneTrackVolume = 0.4f;

    [Tooltip("Track sound pitch with one track active.")]
    public float oneTrackPitch = 0.9f;

    [Tooltip("Track sound volume with both tracks active.")]
    [Range(0f, 1f)]
    public float bothTracksVolume = 0.8f;

    [Tooltip("Track sound pitch with both tracks active.")]
    public float bothTracksPitch = 1.05f;

    [Tooltip("How fast audio volume/pitch fades in and out.")]
    public float audioFadeSpeed = 6f;

    [Header("Haptic Feedback — Idle (always on)")]
    [Tooltip("Low motor intensity when nothing is being operated.")]
    [Range(0f, 1f)]
    public float hapticIdleLow = 0.03f;

    [Tooltip("High motor intensity when nothing is being operated.")]
    [Range(0f, 1f)]
    public float hapticIdleHigh = 0.01f;

    [Header("Haptic Feedback — Swing")]
    [Range(0f, 1f)] public float hapticSwingLow = 0.08f;
    [Range(0f, 1f)] public float hapticSwingHigh = 0.04f;

    [Header("Haptic Feedback — Boom")]
    [Range(0f, 1f)] public float hapticBoomLow = 0.10f;
    [Range(0f, 1f)] public float hapticBoomHigh = 0.03f;

    [Header("Haptic Feedback — Arm")]
    [Range(0f, 1f)] public float hapticArmLow = 0.09f;
    [Range(0f, 1f)] public float hapticArmHigh = 0.04f;

    [Header("Haptic Feedback — Bucket")]
    [Range(0f, 1f)] public float hapticBucketLow = 0.07f;
    [Range(0f, 1f)] public float hapticBucketHigh = 0.05f;

    [Header("Haptic Feedback — One Track")]
    [Range(0f, 1f)] public float hapticOneTrackLow = 0.15f;
    [Range(0f, 1f)] public float hapticOneTrackHigh = 0.08f;

    [Header("Haptic Feedback — Both Tracks")]
    [Range(0f, 1f)] public float hapticBothTracksLow = 0.25f;
    [Range(0f, 1f)] public float hapticBothTracksHigh = 0.12f;

    [Header("Haptic Feedback — General")]
    [Tooltip("How fast haptic motors ramp between states (higher = snappier).")]
    public float hapticFadeSpeed = 8f;

    private float currentHapticLow = 0f;
    private float currentHapticHigh = 0f;

    private DriveParams driveParams;
    private InputEvents inputEvents = new InputEvents();
    public Excavator excavator;

    private JointParams swingParams;
    private JointParams boomParams;
    private JointParams armParams;
    private JointParams bucketParams;

    private LeverAngles rightOperationLeverAngles = new LeverAngles(0F, 0F);
    private LeverAngles leftOperationLeverAngles = new LeverAngles(0F, 0F);
    private LeverAngles rightTravelLeverAngles = new LeverAngles(0F, 0F);
    private LeverAngles leftTravelLeverAngles = new LeverAngles(0F, 0F);

    private const float GAMEPAD_DEADZONE = 0.15f;

    private void Awake()
    {
        excavator = new Excavator(transform.root.gameObject);
    }

    void Start()
    {
        float mass = gameObject.GetComponent<Rigidbody>().mass;
        driveParams = new DriveParams(mass, maxSpeed, creepSpeed,
                                       initialAccel, deltaAccel, maxAccel, deltaDirection);
        RebuildJointParams();

        if (engineIdleAudio != null)
        {
            engineIdleAudio.loop = true;
            engineIdleAudio.volume = engineIdleVolume;
            engineIdleAudio.pitch = engineIdlePitch;
            engineIdleAudio.Stop();
            engineIdleAudio.Play();
        }

        if (trackMovingAudio != null)
        {
            trackMovingAudio.loop = true;
            trackMovingAudio.volume = 0f;
            trackMovingAudio.pitch = oneTrackPitch;
            trackMovingAudio.Stop();
            trackMovingAudio.Play();
        }
    }

    private void RebuildJointParams()
    {
        swingParams = new JointParams(swingHydraulicForce, swingEffectiveMass,
                                        swingLeverArm, swingDamping, swingMaxAngularSpeed);
        boomParams = new JointParams(boomHydraulicForce, boomEffectiveMass,
                                        boomLeverArm, boomDamping, boomMaxAngularSpeed);
        armParams = new JointParams(armHydraulicForce, armEffectiveMass,
                                        armLeverArm, armDamping, armMaxAngularSpeed);
        bucketParams = new JointParams(bucketHydraulicForce, bucketEffectiveMass,
                                        bucketLeverArm, bucketDamping, bucketMaxAngularSpeed);
    }

    private void ProcessKeyEvents(InputEvents inp)
    {
        var gp = Gamepad.current;
        var kb = Keyboard.current;

        // Get VR Controllers
        var leftVR = XRController.leftHand;
        var rightVR = XRController.rightHand;

        // 1. VR Controller Input
        if (leftVR != null || rightVR != null)
        {
            if (leftVR != null)
            {
                // Left Thumbstick: Swing and Arm
                var leftStick = leftVR.TryGetChildControl<Vector2Control>("thumbstick");
                if (leftStick != null)
                {
                    float leftX = leftStick.x.ReadValue();
                    float leftY = leftStick.y.ReadValue();
                    if (Mathf.Abs(leftX) < GAMEPAD_DEADZONE) leftX = 0f;
                    if (Mathf.Abs(leftY) < GAMEPAD_DEADZONE) leftY = 0f;

                    if (Mathf.Abs(leftX) >= Mathf.Abs(leftY))
                    { if (!Mathf.Approximately(leftX, 0f)) inp.swing = leftX; }
                    else
                    { if (!Mathf.Approximately(leftY, 0f)) inp.arm = leftY; }
                }

                // Left Track: Y (Secondary) = Forward, X (Primary) = Backward
                var leftYBtn = leftVR.TryGetChildControl<ButtonControl>("secondaryButton");
                var leftXBtn = leftVR.TryGetChildControl<ButtonControl>("primaryButton");

                if (leftYBtn != null && leftYBtn.isPressed) { inp.trackLeft = 1f; }
                else if (leftXBtn != null && leftXBtn.isPressed) { inp.trackLeft = -1f; }
            }

            if (rightVR != null)
            {
                // Right Thumbstick: Bucket and Boom
                var rightStick = rightVR.TryGetChildControl<Vector2Control>("thumbstick");
                if (rightStick != null)
                {
                    float rightX = rightStick.x.ReadValue();
                    float rightY = rightStick.y.ReadValue();
                    if (Mathf.Abs(rightX) < GAMEPAD_DEADZONE) rightX = 0f;
                    if (Mathf.Abs(rightY) < GAMEPAD_DEADZONE) rightY = 0f;

                    if (Mathf.Abs(rightY) >= Mathf.Abs(rightX))
                    { if (!Mathf.Approximately(rightY, 0f)) inp.boom = rightY; }
                    else
                    { if (!Mathf.Approximately(rightX, 0f)) inp.bucket = rightX; }
                }

                // Right Track: B (Secondary/Top) = Forward, A (Primary/Bottom) = Backward
                // Note: UI shows "Y/X", but right controller physical buttons are B/A
                var rightBBtn = rightVR.TryGetChildControl<ButtonControl>("secondaryButton");
                var rightABtn = rightVR.TryGetChildControl<ButtonControl>("primaryButton");

                if (rightBBtn != null && rightBBtn.isPressed) { inp.trackRight = 1f; }
                else if (rightABtn != null && rightABtn.isPressed) { inp.trackRight = -1f; }
            }
        }
        // 2. Gamepad Input
        else if (gp != null)
        {
            float leftX = gp.leftStick.x.ReadValue();
            float leftY = gp.leftStick.y.ReadValue();
            if (Mathf.Abs(leftX) < GAMEPAD_DEADZONE) leftX = 0f;
            if (Mathf.Abs(leftY) < GAMEPAD_DEADZONE) leftY = 0f;

            if (Mathf.Abs(leftX) >= Mathf.Abs(leftY))
            { if (!Mathf.Approximately(leftX, 0f)) inp.swing = leftX; }
            else
            { if (!Mathf.Approximately(leftY, 0f)) inp.arm = leftY; }

            float rightX = gp.rightStick.x.ReadValue();
            float rightY = gp.rightStick.y.ReadValue();
            if (Mathf.Abs(rightX) < GAMEPAD_DEADZONE) rightX = 0f;
            if (Mathf.Abs(rightY) < GAMEPAD_DEADZONE) rightY = 0f;

            if (Mathf.Abs(rightY) >= Mathf.Abs(rightX))
            { if (!Mathf.Approximately(rightY, 0f)) inp.boom = rightY; }
            else
            { if (!Mathf.Approximately(rightX, 0f)) inp.bucket = rightX; }

            if (gp.rightShoulder.isPressed) { inp.trackRight = 1f; }
            else if (gp.rightTrigger.isPressed) { inp.trackRight = -1f; }

            if (gp.leftShoulder.isPressed) { inp.trackLeft = 1f; }
            else if (gp.leftTrigger.isPressed) { inp.trackLeft = -1f; }
        }
        // 3. Keyboard Input
        else if (kb != null)
        {
            if (kb.dKey.isPressed) { inp.swing = 1f; }
            else if (kb.aKey.isPressed) { inp.swing = -1f; }
            else if (kb.wKey.isPressed) { inp.arm = 1f; }
            else if (kb.sKey.isPressed) { inp.arm = -1f; }

            if (kb.iKey.isPressed) { inp.boom = 1f; }
            else if (kb.kKey.isPressed) { inp.boom = -1f; }
            else if (kb.lKey.isPressed) { inp.bucket = 1f; }
            else if (kb.jKey.isPressed) { inp.bucket = -1f; }

            if (kb.hKey.isPressed) { inp.trackRight = 1f; }
            else if (kb.nKey.isPressed) { inp.trackRight = -1f; }

            if (kb.fKey.isPressed) { inp.trackLeft = 1f; }
            else if (kb.cKey.isPressed) { inp.trackLeft = -1f; }
        }
    }

    private void UpdateAudio(float leftTrack, float rightTrack, float dt)
    {
        int activeTracks = 0;
        if (!Mathf.Approximately(leftTrack, 0f)) activeTracks++;
        if (!Mathf.Approximately(rightTrack, 0f)) activeTracks++;

        if (trackMovingAudio != null)
        {
            float targetVol, targetPitch;
            if (activeTracks == 2) { targetVol = bothTracksVolume; targetPitch = bothTracksPitch; }
            else if (activeTracks == 1) { targetVol = oneTrackVolume; targetPitch = oneTrackPitch; }
            else { targetVol = 0f; targetPitch = oneTrackPitch; }

            trackMovingAudio.volume = Mathf.Lerp(trackMovingAudio.volume, targetVol, dt * audioFadeSpeed);
            trackMovingAudio.pitch = Mathf.Lerp(trackMovingAudio.pitch, targetPitch, dt * audioFadeSpeed);
        }

        if (engineIdleAudio != null)
        {
            float targetPitch = (activeTracks > 0) ? engineUnderLoadPitch : engineIdlePitch;
            engineIdleAudio.pitch = Mathf.Lerp(engineIdleAudio.pitch, targetPitch, dt * audioFadeSpeed);
        }
    }

    private void UpdateHaptics(float leftTrack, float rightTrack,
                                float inputSwing, float inputBoom,
                                float inputArm, float inputBucket, float dt)
    {
        // Removed the early 'if (gp == null) return;' so math still calculates for VR

        int activeTracks = 0;
        if (!Mathf.Approximately(leftTrack, 0f)) activeTracks++;
        if (!Mathf.Approximately(rightTrack, 0f)) activeTracks++;

        float targetLow, targetHigh;

        if (activeTracks == 2)
        {
            targetLow = hapticBothTracksLow;
            targetHigh = hapticBothTracksHigh;
        }
        else if (activeTracks == 1)
        {
            targetLow = hapticOneTrackLow;
            targetHigh = hapticOneTrackHigh;
        }
        else if (!Mathf.Approximately(inputBucket, 0f))
        {
            targetLow = hapticBucketLow;
            targetHigh = hapticBucketHigh;
        }
        else if (!Mathf.Approximately(inputArm, 0f))
        {
            targetLow = hapticArmLow;
            targetHigh = hapticArmHigh;
        }
        else if (!Mathf.Approximately(inputBoom, 0f))
        {
            targetLow = hapticBoomLow;
            targetHigh = hapticBoomHigh;
        }
        else if (!Mathf.Approximately(inputSwing, 0f))
        {
            targetLow = hapticSwingLow;
            targetHigh = hapticSwingHigh;
        }
        else
        {
            targetLow = hapticIdleLow;
            targetHigh = hapticIdleHigh;
        }

        currentHapticLow = Mathf.Lerp(currentHapticLow, targetLow, dt * hapticFadeSpeed);
        currentHapticHigh = Mathf.Lerp(currentHapticHigh, targetHigh, dt * hapticFadeSpeed);

        // 1. Gamepad Haptics
        var gp = Gamepad.current;
        if (gp != null)
        {
            gp.SetMotorSpeeds(currentHapticLow, currentHapticHigh);
        }

        // 2. VR Haptics
        // VR uses a single amplitude. We take the higher of the two target values.
        float vrAmplitude = Mathf.Clamp01(Mathf.Max(currentHapticLow, currentHapticHigh));

        if (vrAmplitude > 0.01f)
        {
            // Sending an impulse every frame with a duration of dt mimics continuous rumble.
            var leftHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
            if (leftHand.isValid) leftHand.SendHapticImpulse(0, vrAmplitude, dt);

            var rightHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
            if (rightHand.isValid) rightHand.SendHapticImpulse(0, vrAmplitude, dt);
        }
    }

    void OnDisable() { StopHaptics(); }
    void OnDestroy() { StopHaptics(); }

    private void StopHaptics()
    {
        // Stop Gamepad
        var gp = Gamepad.current;
        if (gp != null) gp.SetMotorSpeeds(0f, 0f);

        // Stop VR Controllers
        UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand).StopHaptics();
        UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand).StopHaptics();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        RebuildJointParams();

        if (leftOperationLeverAngles.isOperated() || rightOperationLeverAngles.isOperated())
        {
            excavator.leftOperationLeverRotate(
                -leftOperationLeverAngles.leftRight, -leftOperationLeverAngles.upDown);
            excavator.rightOperationLeverRotate(
                -rightOperationLeverAngles.leftRight, -rightOperationLeverAngles.upDown);
            leftOperationLeverAngles.clear();
            rightOperationLeverAngles.clear();
        }

        if (leftTravelLeverAngles.isOperated() || rightTravelLeverAngles.isOperated())
        {
            excavator.leftTravelLeverRotate(-leftTravelLeverAngles.upDown);
            excavator.rightTravelLeverRotate(-rightTravelLeverAngles.upDown);
            excavator.rightPedalRotate(rightTravelLeverAngles.upDown * 2F);
            excavator.leftPedalRotate(leftTravelLeverAngles.upDown * 2F);
            leftTravelLeverAngles.clear();
            rightTravelLeverAngles.clear();
        }

        ProcessKeyEvents(inputEvents);

        if (lockSwing) 
        { 
            inputEvents.swing = 0f;  
            lockSwing = false;
        }
        if (lockBoom) 
        { 
            inputEvents.boom = 0f;
            lockBoom = false;
        }
        if (lockArm) 
        { 
            inputEvents.arm = 0f; 
            lockArm = false; 
        }
        if (lockBucket) 
        { 
            inputEvents.bucket = 0f; 
            lockBucket = false; 
        }

        float liveSwing = excavator.UpdateSwing(inputEvents.swing, swingParams, dt);
        float liveArm = excavator.UpdateArm(inputEvents.arm, armParams, dt);
        float liveBoom = excavator.UpdateBoom(inputEvents.boom, boomParams, dt);
        float liveBucket = excavator.UpdateBucket(inputEvents.bucket, bucketParams, dt);

        if (!Mathf.Approximately(liveSwing, 0f))
            leftOperationLeverAngles.leftRight = 5F * Mathf.Sign(liveSwing)
                                               * (Mathf.Abs(liveSwing) / swingMaxAngularSpeed);
        if (!Mathf.Approximately(liveArm, 0f))
            leftOperationLeverAngles.upDown = 5F * Mathf.Sign(liveArm)
                                               * (Mathf.Abs(liveArm) / armMaxAngularSpeed);
        if (!Mathf.Approximately(liveBoom, 0f))
            rightOperationLeverAngles.upDown = 5F * Mathf.Sign(liveBoom)
                                               * (Mathf.Abs(liveBoom) / boomMaxAngularSpeed);
        if (!Mathf.Approximately(liveBucket, 0f))
            rightOperationLeverAngles.leftRight = 5F * Mathf.Sign(liveBucket)
                                                * (Mathf.Abs(liveBucket) / bucketMaxAngularSpeed);

        if (inputEvents.trackRight != 0F || inputEvents.trackLeft != 0F)
        {
            rightTravelLeverAngles.upDown = 5F * inputEvents.trackRight;
            leftTravelLeverAngles.upDown = 5F * inputEvents.trackLeft;

            float effLeft = excavator.leftTriggerCheck.isGrounded ? inputEvents.trackLeft : 0f;
            float effRight = excavator.rightTriggerCheck.isGrounded ? inputEvents.trackRight : 0f;

            excavator.Move(effLeft, effRight, trackSteerSpeed, dt, driveParams);
        }

        UpdateAudio(inputEvents.trackLeft, inputEvents.trackRight, dt);
        UpdateHaptics(inputEvents.trackLeft, inputEvents.trackRight,
                      inputEvents.swing, inputEvents.boom,
                      inputEvents.arm, inputEvents.bucket, dt);

        inputEvents.clear();

        if (rightOperationLeverAngles.isOperated() || leftOperationLeverAngles.isOperated())
        {
            excavator.rightOperationLeverRotate(
                rightOperationLeverAngles.leftRight, rightOperationLeverAngles.upDown);
            excavator.leftOperationLeverRotate(
                leftOperationLeverAngles.leftRight, leftOperationLeverAngles.upDown);
        }

        if (rightTravelLeverAngles.isOperated() || leftTravelLeverAngles.isOperated())
        {
            excavator.rightTravelLeverRotate(rightTravelLeverAngles.upDown);
            excavator.leftTravelLeverRotate(leftTravelLeverAngles.upDown);
            excavator.rightPedalRotate(-rightTravelLeverAngles.upDown * 2F);
            excavator.leftPedalRotate(-leftTravelLeverAngles.upDown * 2F);
        }
    }
}