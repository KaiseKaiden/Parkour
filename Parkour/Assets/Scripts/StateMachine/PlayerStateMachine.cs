using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : Observer
{
    const float myGravity = 10.0f;
    const float myMaxVaultDistance = 4.5f;

    public enum eStates : int
    {
        Idle,
        Running,
        Jump,
        SlopeJump,
        Falling,
        CayoteFalling,

        Roll,
        HardLand,
        IdleLand,
        Slide,

        WallRun,
        WallRunH,
        WallJumpRight,
        WallJumpLeft,
        WallRunFall,

        Vault,

        LedgeClimb,


        Count
    }

    List<State> myCachedStates = new List<State>();
    State myCurrentState;

    public eStates myPreviusStateEnum = eStates.Count;
    public eStates myCurrentStateEnum;

    [SerializeField] Animator myPlayerAnimator;
    CharacterController myCharacterController;

    [Space(10)]

    [SerializeField] Transform myCameraTransform;
    [SerializeField] Transform myBodyTransform;
    [SerializeField] Transform myKickTransform;
    [SerializeField] LayerMask myWhatIsGround;
    [SerializeField] LayerMask myWhatIsWall;
    [SerializeField] LayerMask myWhatIsObstacle;
    [SerializeField] LayerMask myWhatIsEnemy;
    [SerializeField] LayerMask myWhatIsSlippy;

    public Vector3 myVelocity;
    Vector3 myStaticCameraEuler;
    Vector3 myStartLocalCameraPosition;

    Vector3 myEdgeClimbPosition;
    float myEdgeClimbSpeed;
    Vector3 myObstacleVaultPosition;

    Vector3 myDesiredAngle;

    float myCurrentHeight;
    float myDesiredFOV;
    float myScreenShakeIntensity;
    float myDesiredBodyXrot;

    Vector3 mySpawnPosition;
    Vector3 mySpawnOrientation;

    [SerializeField] GameObject myLandParticle;
    [SerializeField] ParticleSystem mySpeedLinesParticleSystem;

    float myFlow;
    float myFlowDecreaser;
    const float myMaxFlow = 100.0f;
    FlowBar myFlowBar;

    public void SetFlowDecreaser(float aValue)
    {
        myFlowDecreaser = aValue;
    }

    public float GetFlowDecreaser()
    {
        return myFlowDecreaser;
    }

    public FlowBar GetFlowBar()
    {
        return myFlowBar;
    }

    public float GetFlowPercentage()
    {
        return (myFlow / myMaxFlow);
    }

    public float GetFlowStage()
    {
        return Mathf.Clamp((int)(GetFlowPercentage() / 0.25f), 0.0f, 3.0f);
    }

    public void AddFlowPoint(float aValue)
    {
        SetFlowDecreaser(0.0f);

        myFlow += aValue;
        myFlow = Mathf.Clamp(myFlow, 0.0f, myMaxFlow);

        GetFlowBar().SetFlowPercentage(myFlow / myMaxFlow);
    }

    public void RemoveFlowPoint(float aValue)
    {
        myFlow -= aValue;
        myFlow = Mathf.Clamp(myFlow, 0.0f, myMaxFlow);

        GetFlowBar().SetFlowPercentage(myFlow / myMaxFlow);
    }

    void Start()
    {
        GroundPlayer();

        mySpawnPosition = transform.position;
        mySpawnOrientation = transform.eulerAngles;

        myCharacterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;

        // Add States
        myCachedStates.Add(new PlayerIdleState());
        myCachedStates.Add(new PlayerRunningState());
        myCachedStates.Add(new PlayerJumpState());
        myCachedStates.Add(new PlayerSlopeJumpState());
        myCachedStates.Add(new PlayerFallingState());
        myCachedStates.Add(new PlayerCoyoteFallingState());

        myCachedStates.Add(new PlayerRollLanding());
        myCachedStates.Add(new PlayerHardLanding());
        myCachedStates.Add(new PlayerIdleLanding());
        myCachedStates.Add(new PlayerSlidingState());

        myCachedStates.Add(new PlayerWallRunningState());
        myCachedStates.Add(new PlayerHorizontalWallRunningState());
        myCachedStates.Add(new PlayerWallJumpToRight());
        myCachedStates.Add(new PlayerWallJumpToLeft());
        myCachedStates.Add(new PlayerWallRunningFallingState());

        myCachedStates.Add(new PlayerVaultState());

        myCachedStates.Add(new PlayerLedgeClimbState());

        for (int i = 0; i < myCachedStates.Count; i++)
        {
            myCachedStates[i].Init(this);
        }

        ChangeState(eStates.Idle);

        // Stats
        myDesiredFOV = Camera.main.fieldOfView;
        myStartLocalCameraPosition = myCameraTransform.localPosition;
        myCurrentHeight = 2.0f;

        myFlowBar = GameObject.FindGameObjectWithTag("FlowBar").GetComponent<FlowBar>();

        PostMaster.Instance.Subscribe(eMessage.EdgeClimb, this);
        PostMaster.Instance.Subscribe(eMessage.CheckpointPosition, this);
        PostMaster.Instance.Subscribe(eMessage.CheckpointOrientation, this);
        PostMaster.Instance.Subscribe(eMessage.Respawn, this);
    }

    void GroundPlayer()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit))
        {
            transform.position = hit.point;
        }
    }

    void Update()
    {
        if (myCurrentState != null) myCurrentState.Tick();

        // Locomotion
        if (GetCharacterController().enabled)
        {
            GetCharacterController().Move(myVelocity * Time.deltaTime);
        }

        // FOV
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, myDesiredFOV, Time.deltaTime * 3.5f);

        // CAM SHAKE
        myCameraTransform.transform.localPosition = Vector3.zero;
        myScreenShakeIntensity = Mathf.Lerp(myScreenShakeIntensity, 0.0f, Time.deltaTime * 2.5f);
        if (Time.timeScale > 0.0f) myCameraTransform.transform.localPosition += transform.right * Random.Range(-myScreenShakeIntensity, myScreenShakeIntensity) + transform.up * Random.Range(-myScreenShakeIntensity, myScreenShakeIntensity);

        // Body Rot
        Quaternion rotation = Quaternion.Euler(new Vector3(myDesiredBodyXrot, 0.0f, 0.0f));
        myBodyTransform.transform.localRotation = Quaternion.Lerp(myBodyTransform.transform.localRotation, rotation, 5.0f * Time.deltaTime);

        GetPlayerAnimator().SetBool("isGrounded", IsGrounded());
    }

    public void Respawn()
    {
        myCharacterController.enabled = false;

        transform.position = mySpawnPosition;
        transform.eulerAngles = mySpawnOrientation;
        myVelocity = Vector3.zero;
        ChangeState(eStates.Idle);
        GetPlayerAnimator().SetTrigger("respawn");

        myLookRotY = 0.0f;
        myLookRotX = 0.0f;
        myCameraTransform.localEulerAngles = Vector3.zero;

        myFlow = 0.0f;

        myCharacterController.enabled = true;
    }

    override public void Recive(Message aMsg)
    {
        if (aMsg.GetMsg() == eMessage.EdgeClimb)
        {
            myEdgeClimbPosition = aMsg.GetVector3();
        }
        else if (aMsg.GetMsg() == eMessage.CheckpointPosition)
        {
            mySpawnPosition = aMsg.GetVector3();
        }
        else if (aMsg.GetMsg() == eMessage.CheckpointOrientation)
        {
            mySpawnOrientation = aMsg.GetVector3();
        }
        else if (aMsg.GetMsg() == eMessage.Respawn)
        {
            Respawn();
        }
    }

    public Vector3 GetEdgeClimbPosition()
    {
        return myEdgeClimbPosition;
    }

    public float GetEdgeClimbSpeed()
    {
        return myEdgeClimbSpeed;
    }

    public Vector3 GetObstacleVaultPosition()
    {
        return myObstacleVaultPosition;
    }

    public Vector2 GetInput()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2.ClampMagnitude(input, 1.0f);

        return input;
    }

    public void ChangeState(eStates aState)
    {
        myPreviusStateEnum = myCurrentStateEnum;
        myCurrentStateEnum = aState;

        if (myCurrentState != null) myCurrentState.OnExit();
        myCurrentState = myCachedStates[(int)aState];
        myCurrentState.OnEnter();
    }

    public eStates GetPreviusState()
    {
        return myPreviusStateEnum;
    }

    public eStates GetCurrentState()
    {
        return myCurrentStateEnum;
    }

    float myLookRotY = 0.0f;
    float myLookRotX = 0.0f;

    public void AdjustLookRotation()
    {
        Vector3 bodyEuler = myBodyTransform.eulerAngles;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, myCameraTransform.eulerAngles.y, transform.eulerAngles.z);
        myBodyTransform.eulerAngles = bodyEuler;
    }

    public void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        const float lookSensitivity = 360.0f;

        transform.eulerAngles += new Vector3(0.0f, mouseX, 0.0f) * lookSensitivity * Time.deltaTime;

        myLookRotY += mouseY * lookSensitivity * Time.deltaTime;
        myLookRotY = Mathf.Clamp(myLookRotY, -70.0f, 70.0f);
        myCameraTransform.localEulerAngles = new Vector3(myLookRotY, -myBodyTransform.localEulerAngles.y, myCameraTransform.localEulerAngles.z);

        // Save Camera Euler
        myStaticCameraEuler = myCameraTransform.localEulerAngles;
        myLookRotX = 0.0f;
    }

    public void ForwardLookAround()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        const float lookSensitivity = 360.0f;

        myLookRotX += mouseX * lookSensitivity * Time.deltaTime;
        myLookRotX = Mathf.Clamp(myLookRotX, -50.0f, 50.0f);

        myLookRotY += mouseY * lookSensitivity * Time.deltaTime;
        myLookRotY = Mathf.Clamp(myLookRotY, -70.0f, 70.0f);
        myCameraTransform.localEulerAngles = new Vector3(myLookRotY, myStaticCameraEuler.y + myLookRotX - myBodyTransform.localEulerAngles.y, myCameraTransform.localEulerAngles.z);
    }

    public void AdjustCameraRotation()
    {
        myLookRotY = 0.0f;
        myLookRotX = 0.0f;

        myCameraTransform.localRotation = Quaternion.Lerp(myCameraTransform.localRotation, Quaternion.identity, Time.deltaTime * 5.0f);
    }

    public bool IsGrounded()
    {
        //return myCharacterController.isGrounded;
        return Physics.OverlapSphere(transform.position, GetCharacterController().radius, myWhatIsGround, QueryTriggerInteraction.Ignore).Length > 0;
        //return Physics.OverlapSphere(transform.position + Vector3.down * 0.1f, GetCharacterController().radius - 0.1f, myWhatIsGround).Length > 0;
    }

    public bool HasHitHead()
    {
        return Physics.OverlapSphere(transform.position + Vector3.up * 1.8f, GetCharacterController().radius - 0.1f, myWhatIsWall, QueryTriggerInteraction.Ignore).Length > 0;
    }

    public Animator GetPlayerAnimator()
    {
        return myPlayerAnimator;
    }

    public CharacterController GetCharacterController()
    {
        return myCharacterController;
    }

    public void SetVelocityXZ(float aX, float aZ)
    {
        myVelocity.x = aX;
        myVelocity.z = aZ;
    }

    public void SetVelocityXYZ(float aX, float aY, float aZ)
    {
        myVelocity.x = aX;
        myVelocity.y = aY;
        myVelocity.z = aZ;
    }

    public void SetVelocityY(float aVelocityY)
    {
        myVelocity.y = aVelocityY;
    }

    public void SetGroundedYVelocity()
    {
        myVelocity.y = -0.5f;
    }

    public void GravityTick(float aMultiplier = 1.0f)
    {
        myVelocity.y -= myGravity * aMultiplier * Time.deltaTime;
    }

    public Vector3 GetCurrentVelocity()
    {
        return myVelocity;
    }

    public Vector3 GetCurrentVelocityXZ()
    {
        return new Vector3(myVelocity.x, 0.0f, myVelocity.z);
    }

    public LayerMask GetWallLayerMask()
    {
        return myWhatIsWall;
    }

    public LayerMask GetSlippyLayerMask()
    {
        return myWhatIsSlippy;
    }

    public LayerMask GetGroundLayerMask()
    {
        return myWhatIsGround;
    }

    public bool RaycastForward(out RaycastHit aOutHit)
    {
        //Transform trans = myCameraTransform;
        //trans.eulerAngles = new Vector3(0.0f, trans.eulerAngles.y, 0.0f);

        Vector3 forward = transform.forward;

        if (Physics.Raycast(transform.position + Vector3.up, forward, out aOutHit, 1.0f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    public bool RaycastForLeft(out RaycastHit aOutHit)
    {
        //Transform trans = myCameraTransform;
        //trans.eulerAngles = new Vector3(0.0f, trans.eulerAngles.y, 0.0f);

        Vector3 left = transform.forward - transform.right;
        left.Normalize();

        if (Physics.Raycast(transform.position + Vector3.up, left, out aOutHit, 1.5f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    public bool RaycastForRight(out RaycastHit aOutHit)
    {
        //Transform trans = myCameraTransform;
        //trans.eulerAngles = new Vector3(0.0f, trans.eulerAngles.y, 0.0f);

        Vector3 right = transform.forward + transform.right;
        right.Normalize();

        if (Physics.Raycast(transform.position + Vector3.up, right, out aOutHit, 1.5f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    public bool RaycastLeft(out RaycastHit aOutHit)
    {
        //Transform trans = myCameraTransform;
        //trans.eulerAngles = new Vector3(0.0f, trans.eulerAngles.y, 0.0f);

        Vector3 left = -transform.right;

        if (Physics.Raycast(transform.position + Vector3.up, left, out aOutHit, 1.0f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    public bool RaycastRight(out RaycastHit aOutHit)
    {
        //Transform trans = myCameraTransform;
        //trans.eulerAngles = new Vector3(0.0f, trans.eulerAngles.y, 0.0f);

        Vector3 right = transform.right;

        if (Physics.Raycast(transform.position + Vector3.up, right, out aOutHit, 1.0f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    public bool RaycastFeet()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, transform.forward, 0.5f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    public bool RaycastHipp()
    {
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, 0.5f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    public bool RaycastHead()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 1.9f, transform.forward, 0.5f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    public bool RaycastSlideForward(out RaycastHit aOutHit)
    {
        //Transform trans = myCameraTransform;
        //trans.eulerAngles = new Vector3(0.0f, trans.eulerAngles.y, 0.0f);

        Vector3 forward = myBodyTransform.forward;

        if (Physics.SphereCast(transform.position + (myBodyTransform.up * 0.5f), GetCharacterController().radius * 0.15f, forward, out aOutHit, 1.0f, myWhatIsGround, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    // EEE
    public bool GetEdgeHit()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.up, 1.1f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (!Physics.Raycast(transform.position + Vector3.up * 2.2f, transform.forward, 1.0f, GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position + Vector3.up * 2.2f + transform.forward, Vector3.down, out hit, 1.5f, GetWallLayerMask()))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) > 0.8f)
                {
                    myEdgeClimbPosition = hit.point;
                    //myEdgeClimbSpeed = Mathf.Clamp(hit.distance / 2.5f, 0.5f, 1.0f);

                    return true;
                }
            }
        }

        return false;
    }


    public bool WallRunnLeftTransition()
    {
        RaycastHit hit;

        //Transform trans = myCameraTransform;
        //trans.eulerAngles = new Vector3(0.0f, myCameraTransform.eulerAngles.y, 0.0f);

        return RaycastForLeft(out hit) &&
                   (Vector3.Dot((transform.forward - transform.right).normalized, GetCurrentVelocityXZ().normalized) > 0.0f) &&
                   RaycastLeft(out hit) &&
                   Physics.Raycast(transform.position + Vector3.up * 3.0f, -transform.right, 2.0f, GetWallLayerMask(), QueryTriggerInteraction.Ignore);
    }

    public bool WallRunnRightTransition()
    {
        RaycastHit hit;

        //Transform trans = myCameraTransform;
        //trans.eulerAngles = new Vector3(0.0f, trans.eulerAngles.y, 0.0f);

        return RaycastForRight(out hit) &&
                    (Vector3.Dot((transform.forward + transform.right).normalized, GetCurrentVelocityXZ().normalized) > 0.0f) &&
                    RaycastRight(out hit) &&
                    Physics.Raycast(transform.position + Vector3.up * 3.0f, transform.right, 2.0f, GetWallLayerMask(), QueryTriggerInteraction.Ignore);
    }

    public void SetDesiredFOV(float aFOV)
    {
        myDesiredFOV = aFOV;
    }

    public void SetScreenShakeIntensity(float aIntensity)
    {
        myScreenShakeIntensity = aIntensity;
    }

    public void SetCameraHeight(float aHeight)
    {
        aHeight = Mathf.Clamp(aHeight, 0.6f, 2.0f);
        myCurrentHeight = aHeight;

        float scalar = myCurrentHeight * 0.5f;
        myCameraTransform.localPosition = myStartLocalCameraPosition * scalar;
    }

    public Transform GetCameraTransform()
    {
        return myCameraTransform;
    }

    public void SetHeight(float aHeight)
    {
        aHeight = Mathf.Clamp(aHeight, 0.6f, 2.0f);
        float scalar = aHeight * 0.5f;

        GetCharacterController().height = aHeight;
        GetCharacterController().center = new Vector3(0.0f, scalar, 0.0f);
    }

    public float GetHeight()
    {
        return GetCharacterController().height;
    }

    public bool IsHeadingForObstacle()
    {
        Vector3 left, right;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, transform.forward, out hit, 2.0f, myWhatIsObstacle, QueryTriggerInteraction.Ignore))
        {
            left = hit.transform.position + new Vector3(0.0f, hit.transform.localScale.y * 0.5f, 0.0f) - hit.transform.right * hit.transform.localScale.x * 0.5f;
            right = hit.transform.position + new Vector3(0.0f, hit.transform.localScale.y * 0.5f, 0.0f) + hit.transform.right * hit.transform.localScale.x * 0.5f;
            float obstacleTop = left.y;

            Vector2 obstacleStart = new Vector2(left.x, left.z);
            Vector2 obstacleEnd = new Vector2(right.x, right.z);
            Vector2 myStart = new Vector2(transform.position.x, transform.position.z);
            Vector2 myEnd = new Vector2((transform.position + hit.transform.forward * 2.0f).x, (transform.position + hit.transform.forward * 2.0f).z);
            Vector2 point;
            if (FindIntersection(obstacleStart, obstacleEnd, myStart, myEnd, out point))
            {
                myObstacleVaultPosition = new Vector3(point.x, obstacleTop, point.y);
                Vector3 vaultingDistance = (myObstacleVaultPosition - transform.position);
                vaultingDistance.y = 0.0f;
                vaultingDistance *= 2;
                if (vaultingDistance.magnitude < myMaxVaultDistance)
                {
                    if (Physics.OverlapSphere(transform.position + vaultingDistance + Vector3.up * 0.35f, 0.3f, GetWallLayerMask(), QueryTriggerInteraction.Ignore).Length == 0)
                    {
                        return true;
                    }
                }
            }

            if (FindIntersection(obstacleStart, obstacleEnd, myStart, -myEnd, out point))
            {
                myObstacleVaultPosition = new Vector3(point.x, obstacleTop, point.y);
                Vector3 vaultingDistance = (myObstacleVaultPosition - transform.position);
                vaultingDistance.y = 0.0f;
                vaultingDistance *= 2;
                if (vaultingDistance.magnitude < myMaxVaultDistance)
                {
                    if (Physics.OverlapSphere(transform.position + vaultingDistance + Vector3.up * 0.35f, 0.3f, GetWallLayerMask(), QueryTriggerInteraction.Ignore).Length == 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    bool FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        float x1 = p1.x, y1 = p1.y;
        float x2 = p2.x, y2 = p2.y;
        float x3 = p3.x, y3 = p3.y;
        float x4 = p4.x, y4 = p4.y;

        float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        // Check if the lines are parallel (no intersection)
        if (denominator == 0)
        {
            intersection.x = 0.0f;
            intersection.y = 0.0f;

            return false;
        }

        // Calculate the intersection point
        intersection.x = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denominator;
        intersection.y = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denominator;

        return true;
    }

    public bool GroundIsSlippy()
    {
        //return Physics.OverlapSphere(transform.position + Vector3.up * (GetCharacterController().radius - 0.2f), GetCharacterController().radius, myWhatIsSlippy).Length > 0;
        return Physics.OverlapSphere(transform.position - Vector3.down * 0.1f, GetCharacterController().radius, myWhatIsSlippy, QueryTriggerInteraction.Ignore).Length > 0;
    }

    public void SetBodyRotationX(float aAngle)
    {
        myDesiredBodyXrot = aAngle;
    }

    public void SetDesiredAngle(Vector3 aDesiredAngle)
    {
        myDesiredAngle = aDesiredAngle;
    }

    public Vector3 GetDesiredAngle()
    {
        return myDesiredAngle;
    }

    public void CreateLandParticle()
    {
        Instantiate(myLandParticle, transform.position, Quaternion.identity);
    }

    public void SetSpeedLinesActive(bool aBool)
    {
        var emission = mySpeedLinesParticleSystem.emission;
        emission.enabled = aBool;
    }

    public void AnimImpact()
    {
        myCachedStates[(int)myCurrentStateEnum].AnimImpact();
    }

    public void AnimDone()
    {
        myCachedStates[(int)myCurrentStateEnum].AnimDone();
    }

    public float SphereCastStartToMiddleDistance(Vector3 aStart, Vector3 aHitPos)
    {
        float a = (new Vector2(aHitPos.x, aHitPos.z) - new Vector2(aStart.x, aStart.z)).magnitude;
        float c = (aHitPos - aStart).magnitude;
        float b = Mathf.Sqrt(Mathf.Pow(c, 2) - Mathf.Pow(a, 2));

        float c2 = GetCharacterController().radius;
        float b2 = Mathf.Sqrt(Mathf.Pow(c2, 2) - Mathf.Pow(a, 2));

        return (b - b2);
    }

    public Vector3 slidePos;

    // Todo: Use This Method For Determining Wallclimbing later
    private void OnDrawGizmos()
    {
        float maxClimbHeight = 10.0f;

        RaycastHit sideHit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out sideHit, 2.0f) && !Physics.Raycast(transform.position + Vector3.up * maxClimbHeight, transform.forward, 2.0f))
        {
            RaycastHit heightHit;
            if (Physics.Raycast(transform.position + transform.forward * 2.0f + Vector3.up * maxClimbHeight, Vector3.down, out heightHit))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(new Vector3(sideHit.point.x, heightHit.point.y, sideHit.point.z), new Vector3(0.2f, 0.2f, 0.2f));
            }
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(slidePos, new Vector3(0.2f, 0.2f, 0.2f));
    }
}
