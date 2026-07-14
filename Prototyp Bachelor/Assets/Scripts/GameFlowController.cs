using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private float idleTimer = 0f;

    [Header("Reset-Warnung (Fade to Black)")]
    public CanvasGroup resetWarningFade;
    public float fadeWarningDuration = 5f;
    public float fadeCancelSpeed = 8f;

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

        // Statt InputSystem.onEvent: auf konkrete, bereits im Spiel genutzte Actions lauschen
        SubscribeToAllRelevantActions();
    }

    void OnDisable()
    {
        submit.performed -= OnSubmit;
        submit.Disable();

        UnsubscribeFromAllRelevantActions();
    }

    private void SubscribeToAllRelevantActions()
    {
        // Alle Actions, die im Spiel als "Aktivität" zählen sollen
        flowInput.Player.Move.performed += OnAnyRelevantInput;
        flowInput.Player.Sprint.performed += OnAnyRelevantInput;
        flowInput.Player.Menu.performed += OnAnyRelevantInput;
        flowInput.Player.Look.performed += OnAnyRelevantInput;
        flowInput.UI.NavigateVertical.performed += OnAnyRelevantInput;
        flowInput.UI.NavigateHorizontal.performed += OnAnyRelevantInput;
        flowInput.UI.SelectGenre.performed += OnAnyRelevantInput;
        submit.performed += OnAnyRelevantInput; // Submit zählt ebenfalls als Aktivität

        flowInput.Player.Move.Enable();
        flowInput.Player.Sprint.Enable();
        flowInput.Player.Menu.Enable();
        flowInput.UI.NavigateVertical.Enable();
        flowInput.UI.NavigateHorizontal.Enable();
        flowInput.UI.SelectGenre.Enable();
    }

    private void UnsubscribeFromAllRelevantActions()
    {
        flowInput.Player.Move.performed -= OnAnyRelevantInput;
        flowInput.Player.Sprint.performed -= OnAnyRelevantInput;
        flowInput.Player.Menu.performed -= OnAnyRelevantInput;
        flowInput.Player.Look.performed -= OnAnyRelevantInput;
        flowInput.UI.NavigateVertical.performed -= OnAnyRelevantInput;
        flowInput.UI.NavigateHorizontal.performed -= OnAnyRelevantInput;
        flowInput.UI.SelectGenre.performed -= OnAnyRelevantInput;
        submit.performed -= OnAnyRelevantInput;
    }

    void Start()
    {
        resetWarningFade.alpha = 0.5f;
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
        else
        {
            idleTimer += Time.deltaTime;
        }



        if (!isFadingToReset && idleTimer >= idleResetTime)
        {
            StartFadeWarning();
        }

        if (isFadingToReset)
        {
            UpdateFadeWarning();
        }
    }

    private void OnAnyRelevantInput(InputAction.CallbackContext context)
    {
        idleTimer = 0f;
        resetWarningFade.gameObject.SetActive(false);

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
        {
            resetWarningFade.gameObject.SetActive(true);
            resetWarningFade.blocksRaycasts = true;
        }
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

        SoundManager.PlayMenuSound(MenuSoundType.MAINMENU, 1f, true);

        startPauseCanvas.SetActive(true);
        parameterCanvas.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    private void EnterParameterMenu()
    {
        currentState = GameState.ParameterMenu;

        SoundManager.StopAllAudioSources();
        SoundManager.PlayMenuSound(MenuSoundType.STARTGAME, 1f, false);

        startPauseCanvas.SetActive(false);
        parameterCanvas.SetActive(true);
    }

    private void ResetGame()
    {
        flowInput.Player.Disable();
        flowInput.UI.Disable();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}