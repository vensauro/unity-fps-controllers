using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
public class PlaiPlayerMovement : MonoBehaviour
{
  float playerHeight = 2f;
  float playerCenterDistance => playerHeight * 0.5f;
  Rigidbody rb;

  [SerializeField] private Transform orientation;

  [Header("Movement")]
  [SerializeField] float moveSpeed;
  [SerializeField] float airMultiplier = 0.4f;
  [SerializeField] float groundMultiplier = 10f;

  [Header("Sprinting")]
  [SerializeField] float walkSpeed = 4f;
  [SerializeField] float sprintSpeed = 6f;
  [SerializeField] float acceleration = 10f;


  [Header("Jumping")]
  public float jumpForce = 5f;

  [Header("Keybinds")]
  // [SerializeField] KeyCode jumpKey = KeyCode.Space;
  [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

  [Header("Drag")]
  float groundDrag = 6f;
  float airDrag = 2f;

  float horizontalMovement, verticalMovement;
  Vector3 inputDirection;
  Vector3 moveDirection;

  [Header("Ground Detection")]
  [SerializeField] Transform groundCheck;
  [SerializeField] LayerMask groundLayer;

  [ShowNonSerializedField] private bool isGrounded;
  float groundDistance = 0.4f;


  [Header("Slope and Stairs")]
  [SerializeField] private float maxSlopeAngle = 45;
  [SerializeField] private float stepHeight = 0.3f;
  [SerializeField] private float climbStepForce = 20f;
  [SerializeField] private float groundCheckDistance = 1f;
  [SerializeField] private float stairOffset = .3f;
  [ShowNonSerializedField] private bool hasSomethingLower;
  [ShowNonSerializedField] private bool hasSomethingUpper;
  [ShowNonSerializedField] private bool isVerticalLower;
  RaycastHit slopeHit;

  [Header("Debug")]
  public bool drawIsGroundDetector = true;
  public bool drawDirection = true;
  public bool drawStairsDebug = true;

  private void Start()
  {
    rb = GetComponent<Rigidbody>();
    rb.freezeRotation = true;
  }

  private void Update()
  {
    isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
    GetInput();
    ControlDrag();
    ControlSpeed();

    if (Input.GetButtonDown("Jump") && isGrounded)
      Jump();
  }

  private void OnDrawGizmos()
  {
    if (drawIsGroundDetector)
      Gizmos.DrawSphere(groundCheck.position, groundDistance);

    if (drawDirection)
    {
      Gizmos.color = Color.magenta;

      Vector3 direction = moveDirection * 5;
      Gizmos.DrawLine(transform.position, transform.position + direction);
    }

    if (drawStairsDebug)
    {
      Gizmos.color = Color.yellow;

      Gizmos.DrawRay(groundCheck.position + Vector3.up * 0.05f, inputDirection);
      Gizmos.DrawRay(groundCheck.position + Vector3.up * stepHeight, inputDirection);

      Gizmos.DrawSphere(groundCheck.position + Vector3.up * 0.05f + inputDirection * groundCheckDistance, .1f);
      Gizmos.DrawSphere(groundCheck.position + Vector3.up * (stepHeight + 0.05f) + inputDirection * (groundCheckDistance + stairOffset), .1f);
    }
  }

  private bool OnSlope()
  {
    var hasHit = Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerCenterDistance + .5f);
    return hasHit && slopeHit.normal != Vector3.up;
  }

  private Vector3 GetMoveDirection()
  {
    var isInSlope = OnSlope();
    var slopeMoveDirection = Vector3.ProjectOnPlane(inputDirection, slopeHit.normal).normalized;

    var angle = Vector3.Angle(slopeMoveDirection, inputDirection);
    print(angle);

    if (isGrounded && isInSlope && angle < maxSlopeAngle)
      return slopeMoveDirection;

    return inputDirection;
  }

  private void Jump()
  {
    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
  }

  private void ControlSpeed()
  {
    var targetSpeed = Input.GetKey(sprintKey) && isGrounded ? sprintSpeed : walkSpeed;
    moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, acceleration * Time.deltaTime);
  }

  private void ControlDrag()
  {
    rb.drag = isGrounded ? groundDrag : airDrag;
  }

  private void GetInput()
  {
    horizontalMovement = Input.GetAxisRaw("Horizontal");
    verticalMovement = Input.GetAxisRaw("Vertical");

    inputDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    inputDirection.Normalize();
  }

  private void FixedUpdate()
  {
    StepClimb();

    var multiplier = isGrounded ? groundMultiplier : airMultiplier;
    moveDirection = GetMoveDirection();

    rb.AddForce(moveDirection * moveSpeed * multiplier, ForceMode.Acceleration);
  }

  void StepClimb()
  {
    Ray rayLower = new Ray(groundCheck.position + Vector3.up * 0.05f, inputDirection);
    Ray rayUpper = new Ray(groundCheck.position + Vector3.up * (stepHeight + 0.05f), inputDirection);

    hasSomethingLower = Physics.Raycast(rayLower, out var hitLower, groundCheckDistance);
    hasSomethingUpper = Physics.Raycast(rayUpper, out var hitUpper, groundCheckDistance + stairOffset);
    isVerticalLower = hasSomethingLower && Math.Abs(hitLower.normal.y) <= 0.01f;

    if (isVerticalLower && !hasSomethingUpper)
      rb.AddForce(-Physics.gravity * climbStepForce);
  }
}
