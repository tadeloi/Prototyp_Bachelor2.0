using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private float sensX = 1f;
    [SerializeField] private float sensY = 1f;

    [SerializeField] private Transform orientation;

    [SerializeField] private Transform playerFlashlight;

    private InputSystem_Actions userGameInput;
    private InputAction lookAction;

    private float xRotation;
    private float yRotation;

    private void Awake()
    {
        userGameInput = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        lookAction = userGameInput.Player.Look;
        lookAction.Enable();
    }

    private void OnDisable()
    {
        lookAction?.Disable();
        userGameInput?.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 input = lookAction.ReadValue<Vector2>();

        if (input.sqrMagnitude < 0.001f)
            return;

        float cameraX = input.x * sensX * Time.deltaTime;
        float cameraY = input.y * sensY * Time.deltaTime;

        yRotation += cameraX;
        xRotation -= cameraY;
        xRotation = Mathf.Clamp(xRotation, -45f, 45f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);

        playerFlashlight.rotation = Quaternion.Euler(orientation.rotation.eulerAngles.x, orientation.rotation.eulerAngles.y, orientation.rotation.eulerAngles.z);

    }
}