using System.Collections;
using UnityEngine;

public class DriveParams
{
    private float _mass, _maxSpeed, _creepSpeed;
    private float _initialAccel, _deltaAccel, _maxAccel, _deltaDirection;

    public DriveParams(float mass, float maxSpeed, float creepSpeed,
                       float initialAccel, float deltaAccel, float maxAccel, float deltaDirection)
    {
        _mass = mass; _maxSpeed = maxSpeed; _creepSpeed = creepSpeed;
        _initialAccel = initialAccel; _deltaAccel = deltaAccel;
        _maxAccel = maxAccel; _deltaDirection = deltaDirection;
    }

    public float mass { get { return _mass; } }
    public float maxSpeed { get { return _maxSpeed; } }
    public float creepSpeed { get { return _creepSpeed; } }
    public float initialAccel { get { return _initialAccel; } }
    public float deltaAccel { get { return _deltaAccel; } }
    public float maxAccel { get { return _maxAccel; } }
    public float deltaDirection { get { return _deltaDirection; } }
}

public class JointParams
{
    public float hydraulicForce;
    public float effectiveMass;
    public float leverArm;
    public float damping;
    public float maxAngularSpeed;

    public JointParams(float hydraulicForce, float effectiveMass,
                       float leverArm, float damping, float maxAngularSpeed)
    {
        this.hydraulicForce = hydraulicForce;
        this.effectiveMass = effectiveMass;
        this.leverArm = leverArm;
        this.damping = damping;
        this.maxAngularSpeed = maxAngularSpeed;
    }
}

public class TrackTrigger : MonoBehaviour
{
    public bool isGrounded = false;
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground")) isGrounded = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground")) isGrounded = false;
    }
}

public class Excavator
{
    static string excavatorForwardPath = "Armature";
    static string swingAxisPath = "Armature/TurnAxis";
    static string boomAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis";
    static string armAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis/Bone.003/ArmAxis";
    static string bucketAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis/Bone.003/ArmAxis/ArmAxis.002/ArmAxis.003/BucketAxis";
    static string armLinkageAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis/Bone.003/ArmAxis/ArmAxis.002/ArmLinkageAxis";
    static string bucketLinkageAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis/Bone.003/ArmAxis/ArmAxis.002/ArmAxis.003/BucketAxis/ArmAxis.005/BucketLinkageAxis";

    private Transform excavatorForwardAxis;
    private Transform swingAxis, boomAxis, armAxis, bucketAxis;

    private Transform boomCylinderRightAxis1, boomCylinderRight1Target;
    private Transform boomCylinderRightAxis2, boomCylinderRight2Target;
    private Transform boomCylinderLeftAxis1, boomCylinderLeft1Target;
    private Transform boomCylinderLeftAxis2, boomCylinderLeft2Target;
    private Transform armCylinderAxis1, armCylinder1Target;
    private Transform armCylinderAxis2, armCylinder2Target;
    private Transform bucketCylinderAxis1, bucketCylinder1Target;
    private Transform bucketCylinderAxis2, bucketCylinder2Target;
    private Transform armLinkageAxis, armLinkageTarget;
    private Transform bucketLinkageAxis;

    private Transform rightOperationLeverAxis, leftOperationLeverAxis;
    private Transform rightPedalAxis, leftPedalAxis;
    private Transform leftTravelLeverAxis, rightTravelLeverAxis;

    Vector3 joystickRightEulerAngles, joystickLeftEulerAngles;
    Vector3 pedalRightEulerAngles, pedalLeftEulerAngles;
    Vector3 leverRightEulerAngles, leverLeftEulerAngles;

    Quaternion swingAxisInitialQ, boomAxisInitialQ, armAxisInitialQ, bucketAxisInitialQ;

    GameObject arm, boom, body, cabin, rightTrack, leftTrack;

    private GameObject excavator;
    private Rigidbody rb;

    public TrackTrigger leftTriggerCheck;
    public TrackTrigger rightTriggerCheck;

    public float swingVelocity = 0f;
    public float boomVelocity = 0f;
    public float armVelocity = 0f;
    public float bucketVelocity = 0f;

    public float SwingVelocity { get { return swingVelocity; } }
    public float BoomVelocity { get { return boomVelocity; } }
    public float ArmVelocity { get { return armVelocity; } }
    public float BucketVelocity { get { return bucketVelocity; } }

    public Excavator(GameObject excavator)
    {
        this.excavator = excavator;
        Transform t = excavator.transform;

        rb = t.GetComponent<Rigidbody>();
        rb.mass = 20000;
        rb.useGravity = true;

        rb.linearDamping = 0.5f;
        rb.angularDamping = 8f;
        rb.centerOfMass = new Vector3(0f, -1.5f, 0f);

        excavatorForwardAxis = t.Find(excavatorForwardPath);
        swingAxis = t.Find(swingAxisPath);
        boomAxis = t.Find(boomAxisPath);
        armAxis = t.Find(armAxisPath);
        bucketAxis = t.Find(bucketAxisPath);

        swingAxisInitialQ = swingAxis.localRotation;
        boomAxisInitialQ = boomAxis.localRotation;
        armAxisInitialQ = armAxis.localRotation;
        bucketAxisInitialQ = bucketAxis.localRotation;

        armLinkageAxis = t.Find(armLinkageAxisPath);
        bucketLinkageAxis = t.Find(bucketLinkageAxisPath);
        armLinkageTarget = t.Find(bucketLinkageAxisPath + "/ArmLinkageTarget");

        boomCylinderRightAxis1 = t.Find(swingAxisPath + "/Bone.001/Bone.011/BoomCylinderRightAxis1");
        boomCylinderRight1Target = t.Find(boomAxisPath + "/BoomCylinderRight1Target");
        boomCylinderRightAxis2 = t.Find(boomAxisPath + "/Bone.015/BoomCylinderRightAxis2");
        boomCylinderRight2Target = t.Find(swingAxisPath + "/BoomCylinderRight2Target");
        boomCylinderLeftAxis1 = t.Find(swingAxisPath + "/Bone.001/Bone.012/BoomCylinderLeftAxis1");
        boomCylinderLeft1Target = t.Find(boomAxisPath + "/BoomCylinderLeft1Target");
        boomCylinderLeftAxis2 = t.Find(boomAxisPath + "/Bone.016/BoomCylinderLeftAxis2");
        boomCylinderLeft2Target = t.Find(swingAxisPath + "/BoomCylinderLeft2Target");

        armCylinderAxis1 = t.Find(boomAxisPath + "/ArmCylinderAxis1");
        armCylinder1Target = t.Find(armAxisPath + "/ArmCylinder1Target");
        armCylinderAxis2 = t.Find(armAxisPath + "/Bone.010/ArmCylinderAxis2");
        armCylinder2Target = t.Find(boomAxisPath + "/ArmCylinder2Target");

        bucketCylinderAxis1 = t.Find(armAxisPath + "/BucketCylinderAxis1");
        bucketCylinder1Target = t.Find(armLinkageAxisPath + "/BucketCylinder1Target");
        bucketCylinderAxis2 = t.Find(armLinkageAxisPath + "/BucketCylinderAxis2");
        bucketCylinder2Target = t.Find(armAxisPath + "/BucketCylinder2Target");

        rightOperationLeverAxis = t.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.003/JoystickRightAxis");
        leftOperationLeverAxis = t.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.002/JoystickLeftAxis");
        rightPedalAxis = t.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.004/PedalRightAxis");
        leftPedalAxis = t.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.005/PedalLeftAxis");
        rightTravelLeverAxis = t.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.004/PedalRight/LeverRightAxis");
        leftTravelLeverAxis = t.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.005/PedalLeft/LeverLeftAxis");

        joystickLeftEulerAngles = leftOperationLeverAxis.eulerAngles;
        joystickRightEulerAngles = rightOperationLeverAxis.eulerAngles;
        pedalRightEulerAngles = rightPedalAxis.eulerAngles;
        pedalLeftEulerAngles = leftPedalAxis.eulerAngles;
        leverRightEulerAngles = leftTravelLeverAxis.eulerAngles;
        leverLeftEulerAngles = rightTravelLeverAxis.eulerAngles;

        arm = t.Find(armAxisPath + "/Vert.001").gameObject;
        boom = t.Find(boomAxisPath + "/Cube.010").gameObject;
        body = t.Find(swingAxisPath + "/Cube").gameObject;
        cabin = t.Find(swingAxisPath + "/Vert.009").gameObject;
        leftTrack = t.Find("TrackLeft").gameObject;
        rightTrack = t.Find("TrackRight").gameObject;

        arm.AddComponent<MeshCollider>(); arm.GetComponent<MeshCollider>().convex = true;
        boom.AddComponent<MeshCollider>(); boom.GetComponent<MeshCollider>().convex = true;
        body.AddComponent<MeshCollider>(); body.GetComponent<MeshCollider>().convex = true;
        cabin.AddComponent<MeshCollider>(); cabin.GetComponent<MeshCollider>().convex = true;

        leftTrack.AddComponent<BoxCollider>();
        var lt = leftTrack.GetComponent<BoxCollider>().size;
        leftTrack.GetComponent<BoxCollider>().size = new Vector3(lt.x * 4 / 5F, lt.y * 16 / 17F, lt.z);

        rightTrack.AddComponent<BoxCollider>();
        var rt = rightTrack.GetComponent<BoxCollider>().size;
        rightTrack.GetComponent<BoxCollider>().size = new Vector3(rt.x * 4 / 5F, rt.y * 16 / 17F, rt.z);

        leftTriggerCheck = leftTrack.transform.GetChild(0).gameObject.AddComponent<TrackTrigger>();
        rightTriggerCheck = rightTrack.transform.GetChild(0).gameObject.AddComponent<TrackTrigger>();
    }

    private float StepJointPhysics(float velocity, float inputDirection,
                                    JointParams p, float dt)
    {
        if (!Mathf.Approximately(inputDirection, 0f))
        {
            float angularAccelRad = p.hydraulicForce / (p.effectiveMass * p.leverArm);
            float angularAccelDeg = angularAccelRad * Mathf.Rad2Deg;
            velocity += angularAccelDeg * inputDirection * dt;
        }

        velocity -= p.damping * velocity * dt;
        velocity = Mathf.Clamp(velocity, -p.maxAngularSpeed, p.maxAngularSpeed);

        return velocity;
    }

    public float UpdateSwing(float input, JointParams p, float dt)
    {
        swingVelocity = StepJointPhysics(swingVelocity, input, p, dt);
        swingAngle += swingVelocity * dt;
        return swingVelocity;
    }

    public float UpdateBoom(float input, JointParams p, float dt)
    {
        boomVelocity = StepJointPhysics(boomVelocity, input, p, dt);
        boomAngle -= boomVelocity * dt;
        return boomVelocity;
    }

    public float UpdateArm(float input, JointParams p, float dt)
    {
        armVelocity = StepJointPhysics(armVelocity, input, p, dt);
        armAngle += armVelocity * dt;
        return armVelocity;
    }

    public float UpdateBucket(float input, JointParams p, float dt)
    {
        bucketVelocity = StepJointPhysics(bucketVelocity, input, p, dt);
        bucketAngle += bucketVelocity * dt;
        return bucketVelocity;
    }

    public float GetPitchAngle()
    {
        Vector3 machineForward = excavatorForwardAxis.right;
        machineForward.y = 0;
        machineForward.Normalize();
        Vector3 pitchAxis = Vector3.Cross(Vector3.up, machineForward).normalized;
        return Vector3.SignedAngle(Vector3.up, transform.up, pitchAxis);
    }

    public float GetRollAngle()
    {
        Vector3 machineForward = excavatorForwardAxis.right;
        machineForward.y = 0;
        machineForward.Normalize();
        return Vector3.SignedAngle(Vector3.up, transform.up, machineForward);
    }

    //public bool IsLeftTrackGrounded(float maxGroundDistance)
    //{
    //    Vector3 origin = leftTrack.transform.position + Vector3.up * 0.1f;
    //    return Physics.Raycast(origin, Vector3.down, maxGroundDistance);
    //}

    //public bool IsRightTrackGrounded(float maxGroundDistance)
    //{
    //    Vector3 origin = rightTrack.transform.position + Vector3.up * 0.1f;
    //    return Physics.Raycast(origin, Vector3.down, maxGroundDistance);
    //}

    private void OrientHydraulicCylinder(Transform c1, Transform c1t,
                                          Transform c2, Transform c2t, Vector3 up)
    {
        c1.LookAt(c1t, up); c2.LookAt(c2t, up);
        c1.Rotate(new Vector3(90F, 0F, 0F));
        c2.Rotate(new Vector3(90F, 0F, 0F));
    }

    private void OrientLinkage(Transform linkage, Transform target, Vector3 up)
    { linkage.LookAt(target, up); linkage.Rotate(new Vector3(90F, 0F, 0F)); }

    private void OrientArmCylinder()
    {
        OrientHydraulicCylinder(armCylinderAxis1, armCylinder1Target,
                                 armCylinderAxis2, armCylinder2Target, arm.transform.right);
    }

    private void OrientBoomCylinder()
    {
        OrientHydraulicCylinder(boomCylinderRightAxis1, boomCylinderRight1Target,
                                 boomCylinderRightAxis2, boomCylinderRight2Target, arm.transform.right);
        OrientHydraulicCylinder(boomCylinderLeftAxis1, boomCylinderLeft1Target,
                                 boomCylinderLeftAxis2, boomCylinderLeft2Target, arm.transform.right);
    }

    private void OrientBucketCylinder()
    {
        OrientLinkage(armLinkageAxis, armLinkageTarget, arm.transform.right);
        OrientLinkage(bucketLinkageAxis, bucketCylinder1Target, arm.transform.forward);
        OrientHydraulicCylinder(bucketCylinderAxis1, bucketCylinder1Target,
                                 bucketCylinderAxis2, bucketCylinder2Target, boom.transform.right);
    }

    public float swingAngle
    {
        set { swingAxis.localRotation = swingAxisInitialQ; swingAxis.Rotate(0, value, 0); }
        get { return Vector3.SignedAngle(excavatorForwardAxis.right, swingAxis.right, swingAxis.up); }
    }

    public float boomAngle
    {
        set
        {
            if (value >= 0F && value <= 56F)
            { boomAxis.localRotation = boomAxisInitialQ; boomAxis.Rotate(value, 0, 0); OrientBoomCylinder(); }
        }
        get { return Quaternion.Angle(boomAxisInitialQ, boomAxis.localRotation); }
    }

    public float armAngle
    {
        set
        {
            if (value >= 0F && value <= 120F)
            { armAxis.localRotation = armAxisInitialQ; armAxis.Rotate(-value, 0, 0); OrientArmCylinder(); }
        }
        get { return Quaternion.Angle(armAxisInitialQ, armAxis.localRotation); }
    }

    public float bucketAngle
    {
        set
        {
            if (value >= 0F && value <= 160F)
            { bucketAxis.localRotation = bucketAxisInitialQ; bucketAxis.Rotate(value, 0, 0); OrientBucketCylinder(); }
        }
        get { return Quaternion.Angle(bucketAxisInitialQ, bucketAxis.localRotation); }
    }

    public void leftOperationLeverRotate(float lr, float ud) { leftOperationLeverAxis.Rotate(new Vector3(lr, 0, ud)); }
    public void rightOperationLeverRotate(float lr, float ud) { rightOperationLeverAxis.Rotate(new Vector3(lr, 0, ud)); }
    public void leftTravelLeverRotate(float ud) { leftTravelLeverAxis.Rotate(new Vector3(ud, 0, 0)); }
    public void rightTravelLeverRotate(float ud) { rightTravelLeverAxis.Rotate(new Vector3(ud, 0, 0)); }
    public void rightPedalRotate(float ud) { rightPedalAxis.Rotate(new Vector3(ud, 0, 0)); }
    public void leftPedalRotate(float ud) { leftPedalAxis.Rotate(new Vector3(ud, 0, 0)); }

    public Transform transform { get { return excavator.transform; } }

    public void Move(float leftInput, float rightInput,
                     float trackSteerSpeed, float dt, DriveParams driveParams)
    {
        float rawDiff = rightInput - leftInput;

        if (!Mathf.Approximately(rawDiff, 0f))
        {
            float clampedDiff = Mathf.Clamp(rawDiff, -1f, 1f);
            float deltaAngle = -(trackSteerSpeed * dt * clampedDiff);

            float halfSep = Vector3.Distance(leftTrack.transform.position,
                                             rightTrack.transform.position) * 0.5f;

            Vector3 machineRight = (rightTrack.transform.position
                                  - leftTrack.transform.position).normalized;

            float pivotOffset = -halfSep * (rightInput + leftInput) / rawDiff;
            Vector3 pivotPoint = transform.position + machineRight * pivotOffset;

            transform.RotateAround(pivotPoint, Vector3.up, deltaAngle);
        }

        float driveInput = (leftInput + rightInput) * 0.5f;
        if (!Mathf.Approximately(driveInput, 0f))
        {
            // Calculate the total acceleration magnitude first
            float accelMagnitude = driveParams.initialAccel
                                 + (driveParams.maxAccel - driveParams.initialAccel) * Mathf.Abs(driveInput);

            // Apply the forward (+1) or backward (-1) direction to the whole magnitude
            float accel = accelMagnitude * Mathf.Sign(driveInput);

            Vector3 forward = (excavatorForwardAxis.rotation * Vector3.right).normalized;
            rb.AddForceAtPosition(forward * driveParams.mass * accel,
                                  transform.position, ForceMode.Force);
        }
    }

    private bool coroutineIsRunning = false;

    public IEnumerator Reset(float targetSwingAngle = 0F,
                              float targetBoomAngle = 55F,
                              float targetArmAngle = 45F,
                              float targetBucketAngle = 60F)
    {
        while (coroutineIsRunning) { yield return null; }
        coroutineIsRunning = true;

        swingVelocity = boomVelocity = armVelocity = bucketVelocity = 0f;

        float t = 0F;
        float s = swingAngle, bo = boomAngle, ar = armAngle, bu = bucketAngle;

        while (t < 1F)
        {
            t += Time.deltaTime * 0.8F;
            swingAngle = Mathf.Lerp(s, targetSwingAngle, t);
            boomAngle = Mathf.Lerp(bo, targetBoomAngle, t);
            armAngle = Mathf.Lerp(ar, targetArmAngle, t);
            bucketAngle = Mathf.Lerp(bu, targetBucketAngle, t);
            yield return null;
        }

        coroutineIsRunning = false;
    }
}