using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Screenshot : MonoBehaviour
{
    [SerializeField]
    private string path;
    private string savePath;
    [SerializeField]
    [Range(1, 5)]
    private int size = 1;

    public InputSystem_Actions userGameInput;
    private InputAction screenshot;

    void Awake()
    {
        userGameInput = new InputSystem_Actions();
    }

    void Start()
    {

    }

    void OnEnable()
    {
        screenshot = userGameInput.Player.Screenshot;
        screenshot.Enable();
        screenshot.performed += takeScreenshot;
    }

    void OnDisable()
    {
        screenshot.Disable();
    }

    // Update is called once per frame
    void takeScreenshot(InputAction.CallbackContext context)
    {
        savePath = path + "screenshot ";
        savePath += System.Guid.NewGuid().ToString() + ".png";

        ScreenCapture.CaptureScreenshot(savePath, size);
    }
}
