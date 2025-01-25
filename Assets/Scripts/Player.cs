using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class Player: MonoBehaviour, DimensionSwitcher.DimensionSwitch {
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public GameObject playerCamera;
    public GameObject playerDimensionSwitchCollider;

    // Components
    private Rigidbody rb;

    private DimensionSwitcher.DimensionSwitch dimensionSwitchCollider;

    private List<Collision> stayCollisions;

    private Transform cameraTransform;
    private Vector3 gravity;
    private float jumpStrength;
    private Vector3 velocity;
    private bool isGrounded;
    private float groundFriction;
    private float airFriction;
    private float groundAcceleration;
    private float airAcceleration;
    private float maxMoveSpeed;

    private Vector2 inputDirection;
    private bool inputJump;

    private Vector3 dimensionSwitchDirection;

    void Start() {
        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;

        jumpAction.action.performed += OnJump;

        // Get components
        rb = GetComponent<Rigidbody>();
        dimensionSwitchCollider = playerDimensionSwitchCollider.GetComponent<DimensionSwitcher.DimensionSwitch>();

        stayCollisions = new List<Collision>();

        cameraTransform = playerCamera.transform;

        gravity = 18.0f * Vector3.down;
        jumpStrength = 10.0f;
        groundAcceleration = 10.0f;
        groundFriction = 20.0f;
        airFriction = 5.0f;
        airAcceleration = 5.0f;
        maxMoveSpeed = 7.0f;

        velocity = Vector3.zero;
        isGrounded = false;
        dimensionSwitchDirection = Vector3.zero;

        inputDirection = Vector2.zero;
        inputJump = false;
    }

    private static (Vector3, Vector3) getCameraAxes(Transform cameraTransform) {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0.0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0.0f;
        cameraRight.Normalize();

        return (cameraForward, cameraRight);
    }

    void FixedUpdate() {
        // Correct `velocity` based on the actual linear velocity
        velocity = rb.linearVelocity;

        isGrounded = false;
        Vector3 closestGroundContact = Vector3.positiveInfinity;
        float closestDistance = Mathf.Infinity;
        foreach (Collision collision in stayCollisions) {
            foreach (ContactPoint contact in collision.contacts) {
                if (Vector3.Dot(contact.normal, gravity.normalized) > 0.7f) {
                    isGrounded = true;
                    float distance = (contact.point - transform.position).sqrMagnitude;
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestGroundContact = contact.point;
                    }
                }
            }
        }
        stayCollisions.Clear();

        // If the player's current position is above ground
        bool isAboveGround = Physics.Raycast(transform.position, gravity.normalized, 1.5f);
        if (isGrounded && !isAboveGround && dimensionSwitchDirection != Vector3.zero) {
            // If the player's collider is on the ground, but there's no ground
            // directly beneath the player's position we need to move their
            // position so they don't fall after switching back to 3D
            Vector3 displacement = Vector3.Project(closestGroundContact - transform.position, dimensionSwitchDirection);
            //displacement -= Vector3.Project(displacement, dimensionSwitchDirection);
            /*
            transform.position =
                Vector3.Project(closestGroundContact, dimensionSwitchDirection)
                - displacement;
            */
            transform.position += displacement;
        }

        if (!isGrounded) {
            // Apply gravity
            velocity += Time.deltaTime * gravity;
        }

        // Move the player in the input direction, transformed according to the
        // camera's perspective
        float accelerationCoefficient = isGrounded ? groundAcceleration : airAcceleration;

        (Vector3 cameraForward, Vector3 cameraRight) = getCameraAxes(cameraTransform);
        Vector3 moveDirection = inputDirection.x * cameraRight + inputDirection.y * cameraForward;

        Vector3 groundVelocity = velocity;
        groundVelocity.y = 0.0f;

        if (groundVelocity.magnitude < maxMoveSpeed) {
            velocity += Time.deltaTime * accelerationCoefficient * moveDirection;
        }

        // Jump
        if (inputJump && isGrounded) {
            velocity += jumpStrength * Vector3.up;
        }
        inputJump = false;

        // Apply friction to the component of the player's velocity NOT aligned
        // with `gravity` or `moveDirection`.
        float frictionCoefficient = isGrounded ? groundFriction : airFriction;

        Vector3 velocityInGravityDirection =
            //gravity.normalized * Mathf.Abs(Vector3.Dot(velocity, gravity.normalized));
            gravity.normalized * Vector3.Dot(velocity, gravity.normalized);

        Vector3 velocityInMoveDirection =
            moveDirection * Mathf.Max(Vector3.Dot(
                        velocity - velocityInGravityDirection,
                        moveDirection.normalized), 0.0f);

        Vector3 velocitySubjectToFriction =
            velocity - Vector3.ClampMagnitude(velocityInMoveDirection, maxMoveSpeed);
        velocitySubjectToFriction.y = 0.0f;

        Vector3 friction = velocitySubjectToFriction.normalized * frictionCoefficient;

        velocity -= Vector3.ClampMagnitude(
                Time.deltaTime * friction,
                velocitySubjectToFriction.magnitude);

        rb.linearVelocity = velocity;
    }

    public void OnMove(InputAction.CallbackContext c) {
        inputDirection = c.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext c) {
        inputJump = true;
    }

    public void OnCollisionStay(Collision c) {
        stayCollisions.Add(c);
        /*
        foreach (ContactPoint contact in c.contacts) {
            //Debug.Log(contact.point);
        }
        */
    }

    public void dimensionSwitch(
        DimensionSwitcher.Dimension switchTo,
        float progress, Vector3 position, Vector3 direction)
    {
        switch (switchTo) {
            case DimensionSwitcher.Dimension.Two:
                dimensionSwitchDirection = direction;
                break;
            case DimensionSwitcher.Dimension.Three:
                dimensionSwitchDirection = Vector3.zero;
                break;
        }

        dimensionSwitchCollider.dimensionSwitch(switchTo, progress, position, direction);
    }
}
