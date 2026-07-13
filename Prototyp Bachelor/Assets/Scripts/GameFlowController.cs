using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    private enum GameState { StartScreen, ParameterMenu, Exploring }

    [Header("Canvases")]
    public GameObject startPauseCanvas;
    public GameObject parameterCanvas;

    [Header("Player")]
    public GameObject playerObject;
    private PlayerMovement playerMovement;

    [Header("Kamera-Rotation im Startbildschirm")]
    public Transform cameraRotationTarget;
    public float cameraRotationSpeed = 5f;

    [Header("Idle Reset")]
    public float idleResetTime = 90f;
    private float idleTimer = 0f;

    [Header("Reset-Warnung (Fade to Black)")]
    public CanvasGroup resetWarningFade; // schwarzes Vollbild-Image mit CanvasGroup
    public float fadeWarningDuration = 5f;
    public float fadeCancelSpeed = 8f; // wie schnell es beim Abbruch wieder hell wird (Alpha/Sekunde)

    private bool isFadingToReset = false;
    private float fadeTimer = 0f;

    private GameState currentState;

    private InputSystem_Actions flowInput;
    private InputAction submit;

    void Awake()
    {
        flowInput = new InputSystem_Actions();
        playerMovement = playerObject.GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        submit = flowInput.UI.Submit;
        submit.Enable();
        submit.performed += OnSubmit;

        InputSystem.onEvent += OnAnyInputEvent;
    }

    void OnDisable()
    {
        submit.performed -= OnSubmit;
        submit.Disable();

        InputSystem.onEvent -= OnAnyInputEvent;
    }

    void Start()
    {
        EnterStartScreen();

        if (resetWarningFade != null)
        {
            resetWarningFade.alpha = 0f;
            resetWarningFade.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (currentState == GameState.StartScreen && cameraRotationTarget != null)
        {
            cameraRotationTarget.Rotate(Vector3.up, cameraRotationSpeed * Time.deltaTime, Space.World);
        }

        idleTimer += Time.deltaTime;

        if (!isFadingToReset && idleTimer >= idleResetTime)
        {
            StartFadeWarning();
        }

        if (isFadingToReset)
        {
            UpdateFadeWarning();
        }
    }

    private void OnAnyInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (device is Mouse) return;
        idleTimer = 0f;
        Debug.Log($"Detected input!{eventPtr.ToString()}");
        if (isFadingToReset)
        {
            CancelFadeWarning();
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (currentState == GameState.StartScreen)
        {
            EnterParameterMenu();
        }
    }

    private void StartFadeWarning()
    {
        isFadingToReset = true;
        fadeTimer = 0f;

        if (resetWarningFade != null)
            resetWarningFade.blocksRaycasts = true; // verhindert Klicks während der Warnung
    }

    private void UpdateFadeWarning()
    {
        fadeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(fadeTimer / fadeWarningDuration);

        if (resetWarningFade != null)
            resetWarningFade.alpha = t;

        if (t >= 1f)
        {
            ResetGame();
        }
    }

    private void CancelFadeWarning()
    {
        isFadingToReset = false;
        fadeTimer = 0f;

        if (resetWarningFade != null)
        {
            resetWarningFade.blocksRaycasts = false;
            // Nicht sofort auf 0 springen, sondern zügig ausblenden -> fühlt sich weicher an
            StartCoroutine(FadeOutQuickly());
        }
    }

    private System.Collections.IEnumerator FadeOutQuickly()
    {
        while (resetWarningFade.alpha > 0f)
        {
            resetWarningFade.alpha -= fadeCancelSpeed * Time.deltaTime;
            yield return null;
        }
        resetWarningFade.alpha = 0f;
    }

    private void EnterStartScreen()
    {
        currentState = GameState.StartScreen;
        idleTimer = 0f;

        startPauseCanvas.SetActive(true);
        parameterCanvas.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    private void EnterParameterMenu()
    {
        currentState = GameState.ParameterMenu;

        startPauseCanvas.SetActive(false);
        parameterCanvas.SetActive(true);
    }

    private void ResetGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}