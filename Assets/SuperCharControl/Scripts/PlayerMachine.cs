using UnityEngine;
using System.Collections;
using util=PathwaysEngine.Utilities;

/*
 * Example implementation of the SuperStateMachine and SuperCharacterController
 */
[RequireComponent(typeof(SuperCharacterController))]
public class PlayerMachine : SuperStateMachine {

    public Transform AnimatedMesh;

    public float WalkSpeed = 4.0f;
    public float WalkAcceleration = 30.0f;
    public float JumpAcceleration = 5.0f;
    public float JumpHeight = 3.0f;
    public float Gravity = 25.0f;
    public util::key jump, dash, duck;
    public util::axis axisX, axisY, mouseX, mouseY;

    // Add more states by comma separating them
    enum PlayerStates { Idle, Walk, Jump, Fall }

    private SuperCharacterController controller;

    // current velocity
    private Vector3 moveDirection;
    // current direction our character's art is facing
    public Vector3 lookDirection { get; private set; }

    public PlayerMachine() {
        jump = new util::key((n)=>jump.input=n);
        dash = new util::key((n)=>dash.input=n);
        duck = new util::key((n)=>duck.input=n);
        axisX = new util::axis((n)=>axisX.input=n);
        axisY = new util::axis((n)=>axisY.input=n);
        mouseX = new util::axis((n)=>mouseX.input=n);
        mouseY = new util::axis((n)=>mouseY.input=n);
    }

    void Start () {
        // Put any code here you want to run ONCE, when the object is initialized
        // Grab the controller object from our object
        controller = gameObject.GetComponent<SuperCharacterController>();
        // Our character's current facing direction, planar to the ground
        lookDirection = transform.forward;
        // Set our currentState to idle on startup
        currentState = PlayerStates.Idle;
    }

    protected override void EarlyGlobalSuperUpdate() {
        // Rotate out facing direction horizontally based on mouse input
        lookDirection = Quaternion.AngleAxis(mouseX.input,controller.up)*lookDirection;
        // Put any code in here you want to run BEFORE the state's update function.
        // This is run regardless of what state you're in
    }

    protected override void LateGlobalSuperUpdate() {
        // Put any code in here you want to run AFTER the state's update function.
        // This is run regardless of what state you're in

        // Move the player by our velocity every frame
        transform.position += moveDirection * Time.deltaTime;

        // Rotate our mesh to face where we are "looking"
        AnimatedMesh.rotation = Quaternion.LookRotation(lookDirection, controller.up);
    }

    private bool AcquiringGround()
    {
        return controller.currentGround.IsGrounded(false, 0.01f);
    }

    private bool MaintainingGround() {
        return controller.currentGround.IsGrounded(true,0.5f); }

    public void RotateGravity(Vector3 up) {
        lookDirection = Quaternion.FromToRotation(transform.up,up)*lookDirection;}

    /// <summary>
    /// Constructs a vector representing our movement local
    /// to our lookDirection, which is controlled by the camera
    /// </summary>
    private Vector3 LocalMovement() {
        Vector3 right = Vector3.Cross(controller.up, lookDirection);
        Vector3 local = Vector3.zero;
        if (axisX.input!=0f) local += right*axisX.input;
        if (axisY.input!=0f) local += lookDirection*axisY.input;
        return local.normalized;
    }

    // Calculate the initial velocity of a jump based off gravity and desired maximum height attained
    private float CalculateJumpSpeed(float jumpHeight, float gravity) {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    /*void Update () {
     * Update is normally run once on every frame update. We won't be using it
     * in this case, since the SuperCharacterController component sends a callback Update
     * called SuperUpdate. SuperUpdate is recieved by the SuperStateMachine, and then fires
     * further callbacks depending on the state
    }*/

    // Below are the three state functions. Each one is called based on the name of the state,
    // so when currentState = Idle, we call Idle_EnterState. If currentState = Jump, we call
    // Jump_SuperUpdate()
    void Idle_EnterState() {
        controller.EnableSlopeLimit();
        controller.EnableClamping();
    }

    void Idle_SuperUpdate() { // Run every frame we are in the idle state
        if (jump.input) {
            currentState = PlayerStates.Jump; return; }
        if (!MaintainingGround()) {
            currentState = PlayerStates.Fall; return; }
        if (axisX.input!=0f && axisY.input!=0f) {
            currentState = PlayerStates.Walk; return; }
        // Apply friction to slow us to a halt
        moveDirection = Vector3.MoveTowards(
            moveDirection, Vector3.zero, 10f*Time.deltaTime);
    }

    void Idle_ExitState() { } // Run once when we exit the idle state

    void Walk_SuperUpdate() {
        if (jump.input) {
            currentState = PlayerStates.Jump; return; }
        if (!MaintainingGround()) {
            currentState = PlayerStates.Fall; return; }
        if (axisX.input!=0f && axisY.input!=0f) {
            moveDirection = Vector3.MoveTowards(
                moveDirection,LocalMovement()*WalkSpeed,
                WalkAcceleration*Time.deltaTime);
        } else { currentState = PlayerStates.Idle; return; }
    }

    void Jump_EnterState() {
        controller.DisableClamping();
        controller.DisableSlopeLimit();
        moveDirection += controller.up * CalculateJumpSpeed(JumpHeight, Gravity);
    }

    void Jump_SuperUpdate() {
        Vector3 planarMoveDirection = Math3d.ProjectVectorOnPlane(
            controller.up, moveDirection);
        Vector3 verticalMoveDirection = moveDirection - planarMoveDirection;

        if (Vector3.Angle(verticalMoveDirection,controller.up)>90 && AcquiringGround()) {
            moveDirection = planarMoveDirection;
            currentState = PlayerStates.Idle;
            return;
        }

        planarMoveDirection = Vector3.MoveTowards(
            planarMoveDirection, LocalMovement() * WalkSpeed,
            JumpAcceleration * Time.deltaTime);
        verticalMoveDirection -= controller.up * Gravity * Time.deltaTime;

        moveDirection = planarMoveDirection + verticalMoveDirection;
    }

    void Fall_EnterState()
    {
        controller.DisableClamping();
        controller.DisableSlopeLimit();

        // moveDirection = trueVelocity;
    }

    void Fall_SuperUpdate()
    {
        if (AcquiringGround())
        {
            moveDirection = Math3d.ProjectVectorOnPlane(controller.up, moveDirection);
            currentState = PlayerStates.Idle;
            return;
        }

        moveDirection -= controller.up * Gravity * Time.deltaTime;
    }
}
