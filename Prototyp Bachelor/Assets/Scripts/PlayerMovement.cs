using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;
    public Transform orientation;

    [Header("Sprint")]
    public float sprintMultiplier = 2f;
    public float sprintRampDuration = 1f;

    [Header("FOV")]
    public Camera playerCamera;
    public float baseFOV;
    public float sprintFOVMultiplier = 1.2f;
    public float fovRampDuration = 0.2f;

    private Vector3 moveDirection;
    private Vector2 rawInput;

    private bool sprinting;
    private float sprintRampTimer = 0f;
    private float fovRampTimer = 0f;
    private float currentMaxSpeed; // wird pro Frame berechnet, in FixedUpdate genutzt

    public InputSystem_Actions userGameInput;
    private InputAction movement;
    private InputAction menu;
    private InputAction sprint;

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
        baseFOV = playerCamera.fieldOfView;
        currentMaxSpeed = moveSpeed;
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

        sprint = userGameInput.Player.Sprint;
        sprint.Enable();
        sprint.performed += ToggleSprint;
    }

    void OnDisable()
    {
        menu.performed -= ReturnToMenu;
        menu.Disable();

        sprint.performed -= ToggleSprint;
        sprint.Disable();

        movement.Disable();
    }

    void Update()
    {
        rb.linearDamping = groundDrag;

        UpdateSprintRamp(); // aktualisiert currentMaxSpeed
        SpeedControl();     // Sicherheitsnetz, greift z.B. bei Stößen o.ä.
        UpdateFOV();
    }

    void FixedUpdate()
    {
        moveDirection = orientation.forward * rawInput.y + orientation.right * rawInput.x;
        // Antriebskraft nutzt jetzt currentMaxSpeed statt fix moveSpeed
        rb.AddForce(currentMaxSpeed * moveDirection.normalized, ForceMode.Force);
    }

    private void ReturnToMenu(InputAction.CallbackContext context)
    {
        ParameterCanvas.SetActive(true);
        this.OnDisable();
    }

    private void UpdateSprintRamp()
    {
        float direction = sprinting ? 1f : -1f;
        sprintRampTimer = Mathf.Clamp(sprintRampTimer + direction * Time.deltaTime, 0f, sprintRampDuration);

        float rampT = sprintRampTimer / sprintRampDuration;
        currentMaxSpeed = Mathf.Lerp(moveSpeed, moveSpeed * sprintMultiplier, rampT);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > currentMaxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMaxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void UpdateFOV()
    {
        if (playerCamera == null) return;

        Vector3 flatVel = new(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float currentSpeed = flatVel.magnitude;

        bool overSpeeding = currentSpeed > moveSpeed;

        float direction = overSpeeding ? 1f : -1f;
        fovRampTimer = Mathf.Clamp(fovRampTimer + direction * Time.deltaTime, 0f, fovRampDuration);

        float fovT = fovRampTimer / fovRampDuration;
        fovT = 1f - Mathf.Pow(1f - fovT, 3f); // Ease-Out Cubic: schneller Start, sanftes Ende
        float targetFOV = Mathf.Lerp(baseFOV, baseFOV * sprintFOVMultiplier, fovT);


        playerCamera.fieldOfView = targetFOV;
    }

    private void ToggleSprint(InputAction.CallbackContext context)
    {
        sprinting = !sprinting;
        baseFOV = playerCamera.fieldOfView;
        Debug.Log("Toggling Sprinting to: " + sprinting);
    }
}