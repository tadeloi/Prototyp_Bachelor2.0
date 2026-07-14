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

    [Header("Footstep Sound")]
    [Tooltip("Wird gesetzt, sobald der Spieler eine Category auswählt (z.B. über ParameterSelection). Bestimmt, welcher Walking-/Sprinting-Clip abgespielt wird.")]
    public Categories playerCategory;
    [Tooltip("Ab welcher horizontalen Geschwindigkeit der Charakter als 'in Bewegung' gilt.")]
    [SerializeField] private float movementSoundThreshold = 0.1f;
    [Tooltip("Dauer der Überblendung zwischen Walking- und Sprinting-Sound bzw. beim Stoppen.")]
    [SerializeField] private float footstepFadeDuration = 0.25f;

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

    //Shows whether the SoundOption of ParameterSelection is active or not. If the background Option is enabled, it is active, and the player will hear footsteps.
    public bool soundIsActive = false;
    //Shows whether the sound is currently playing or not. If the player is moving, it is true, if the player is standing still, it is false.
    private bool soundIsPlaying = false;

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
        movement.performed += OnMovementPerformed;
        movement.canceled += OnMovementCanceled;

        menu = userGameInput.Player.Menu;
        menu.Enable();
        menu.performed += ReturnToMenu;

        sprint = userGameInput.Player.Sprint;
        sprint.Enable();
        sprint.performed += ToggleSprint;
    }

    void OnDisable()
    {
        movement.performed -= OnMovementPerformed;
        movement.canceled -= OnMovementCanceled;
        movement.Disable();

        menu.performed -= ReturnToMenu;
        menu.Disable();

        sprint.performed -= ToggleSprint;
        sprint.Disable();

        // Sicherstellen, dass der Footstep-Sound aufhört, falls das Objekt deaktiviert wird.
        if (soundIsPlaying)
        {
            SoundManager.StopFootstepSound(footstepFadeDuration);
            soundIsPlaying = false;
        }
    }

    private void OnMovementPerformed(InputAction.CallbackContext ctx) => rawInput = ctx.ReadValue<Vector2>();
    private void OnMovementCanceled(InputAction.CallbackContext ctx) => rawInput = Vector2.zero;

    void Update()
    {
        rb.linearDamping = groundDrag;

        UpdateSprintRamp(); // aktualisiert currentMaxSpeed
        SpeedControl();     // Sicherheitsnetz, greift z.B. bei Stößen o.ä.
        UpdateFOV();
        UpdateFootstepSound();
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
        this.enabled = false;
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

    /// <summary>
    /// Startet/stoppt bzw. blendet zwischen Walking- und Sprinting-Sound um,
    /// basierend auf der tatsächlichen horizontalen Rigidbody-Geschwindigkeit.
    /// </summary>
    private void UpdateFootstepSound()
    {
        if (!soundIsActive)
        {
            if (soundIsPlaying)
            {
                SoundManager.StopFootstepSound(footstepFadeDuration);
                soundIsPlaying = false;
            }
            return;
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool isMoving = flatVel.magnitude > movementSoundThreshold;

        if (isMoving)
        {
            SoundType footstepType = sprinting ? SoundType.SPRINTING : SoundType.WALKING;
            SoundManager.PlayFootstepSound(playerCategory, footstepType, 1f, footstepFadeDuration);
            soundIsPlaying = true;
        }
        else if (soundIsPlaying)
        {
            SoundManager.StopFootstepSound(footstepFadeDuration);
            soundIsPlaying = false;
        }
    }

    private void ToggleSprint(InputAction.CallbackContext context)
    {
        sprinting = !sprinting;
        baseFOV = playerCamera.fieldOfView;
        Debug.Log("Toggling Sprinting to: " + sprinting);
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

    //Shows whether the SoundOption of ParameterSelection is active or not. If the background Option is enabled, it is active, and the player will hear footsteps.
    public bool soundIsActive = false;
    //Shows whether the sound is currently playing or not. If the player is moving, it is true, if the player is standing still, it is false.
    private bool soundIsPlaying = false;

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
        movement.performed += OnMovementPerformed;
        movement.canceled += OnMovementCanceled;

        menu = userGameInput.Player.Menu;
        menu.Enable();
        menu.performed += ReturnToMenu;

        sprint = userGameInput.Player.Sprint;
        sprint.Enable();
        sprint.performed += ToggleSprint;
    }

    void OnDisable()
    {
        movement.performed -= OnMovementPerformed;
        movement.canceled -= OnMovementCanceled;
        movement.Disable();

        menu.performed -= ReturnToMenu;
        menu.Disable();

        sprint.performed -= ToggleSprint;
        sprint.Disable();
    }

    private void OnMovementPerformed(InputAction.CallbackContext ctx) => rawInput = ctx.ReadValue<Vector2>();
    private void OnMovementCanceled(InputAction.CallbackContext ctx) => rawInput = Vector2.zero;

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
        this.enabled = false;
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
}*/