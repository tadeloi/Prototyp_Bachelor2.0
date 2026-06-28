using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{

    [SerializeField] float sensX;
    [SerializeField] float sensY;

    public Transform orientation;

    public InputSystem_Actions userGameInput;
    private InputAction horizontal;

    float xRotation;
    float yRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        userGameInput = new InputSystem_Actions();
    }

    void OnEnable()
    {
        horizontal = userGameInput.Player.Look;
        horizontal.Enable();
        horizontal.performed += CameraMovement;
    }

    void OnDisable()
    {
        horizontal.Disable();
    }

    void CameraMovement(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        int direction = input.x > 0.1f ? 1 : input.x < -0.1f ? -1 : 0;
        if (direction == 0) return;

        float cameraX = input.x * Time.deltaTime * sensX;
        float cameraY = input.y * Time.deltaTime * sensY;

        yRotation += cameraX;
        xRotation -= cameraY;
        xRotation = Mathf.Clamp(xRotation, -5f, 45f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

}
