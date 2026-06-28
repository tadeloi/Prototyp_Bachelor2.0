using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public Transform orientation;

    private Vector3 moveDirection;
    private Vector2 rawInput;

    public InputSystem_Actions userGameInput;
    private InputAction movement;

    private Rigidbody rb;

    void Awake()
    {
        userGameInput = new InputSystem_Actions();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        movement = userGameInput.Player.Move;
        movement.Enable();
        movement.performed += ctx => rawInput = ctx.ReadValue<Vector2>();
        movement.canceled += ctx => rawInput = Vector2.zero;
    }

    void OnDisable()
    {
        movement.Disable();
    }

    void Update()
    {
        rb.linearDamping = groundDrag;
        SpeedControl();
    }

    void FixedUpdate()
    {
        moveDirection = orientation.forward * rawInput.y + orientation.right * rawInput.x;
        rb.AddForce(moveSpeed * moveDirection.normalized, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
}


/* Saving old Code

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    public InputSystem_Actions userGameInput;
    private InputAction movement;

    Rigidbody rb;

    Vector2 moveDirection;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    void Awake()
    {
        userGameInput = new InputSystem_Actions();
    }

    void OnEnable()
    {
        movement = userGameInput.Player.Move;
        movement.Enable();
        movement.performed += PlayerMove;
    }

    void OnDisable()
    {
        movement.Disable();
    }

    void PlayerMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        Debug.Log("Movement detected! X: " + input.x + " Y: " + input.y);
        int horizontalInput = input.x > 0.1f ? 1 : input.x < -0.1f ? -1 : 0;
        int verticalInput = input.y > 0.1f ? 1 : input.y < -0.1f ? -1 : 0;
        if (horizontalInput == 0 && verticalInput == 0) return;
        Debug.Log("Trying to move player");
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        Debug.Log("moveDirection: " + moveDirection.ToString());
        rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
        Debug.Log("Added force to RigidBody");
    }
}*/