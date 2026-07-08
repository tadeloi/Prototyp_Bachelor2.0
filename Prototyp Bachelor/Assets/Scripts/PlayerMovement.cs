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

    private InputAction menu;

    private Rigidbody rb;

    [SerializeField] private GameObject ParameterCanvas;

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

        menu = userGameInput.Player.Menu;
        menu.Enable();
        menu.performed += ReturnToMenu;
    }

    void OnDisable()
    {
        movement.Disable();
        menu.Disable();
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

    private void ReturnToMenu(InputAction.CallbackContext context)
    {
        ParameterCanvas.SetActive(true);
        this.OnDisable();
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