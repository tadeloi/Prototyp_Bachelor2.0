using System.Collections;
using UnityEngine;

public class ScaleController : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public GameObject playerObject;
    public Camera playerCamera;

    [SerializeField] private float transitionDuration = 1f;

    [Header("Ground Detection")]
    [Tooltip("Layer, auf dem der Boden liegt. Wird per Raycast aus dem Charakterzentrum nach unten gesucht.")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundRayMaxDistance = 50f;

    private Coroutine cameraTransitionCoroutine;
    private Rigidbody playerRigidbody;

    public float[] movementSpeeds;
    public float[] FOVs;
    public float[] sizes;

    private void Awake()
    {
        if (playerObject != null)
            playerRigidbody = playerObject.GetComponent<Rigidbody>();
    }

    // Setzt moveSpeed sofort, Scale/Position/FOV laufen über die Transition
    public void UpdateScale(Categories category)
    {
        playerMovement.moveSpeed = movementSpeeds[(int)category];
        SetScale(category);
    }

    // Startet den weichen Übergang und bricht einen evtl. laufenden vorher ab
    public void SetScale(Categories category)
    {
        if (cameraTransitionCoroutine != null)
            StopCoroutine(cameraTransitionCoroutine);

        cameraTransitionCoroutine = StartCoroutine(TransitionPlayer(category));
    }

    private IEnumerator TransitionPlayer(Categories category)
    {
        float elapsed = 0f;

        float targetSize = sizes[(int)category];

        Vector3 startScale = playerObject.transform.localScale;
        Vector3 targetScale = new Vector3(targetSize, targetSize, targetSize);

        Vector3 startPos = playerObject.transform.position;

        // Tatsächliche Bodenhöhe per Raycast aus dem Zentrum des Charakters ermitteln,
        // statt sie nur aus der Zielgröße zu errechnen. Verhindert, dass der Charakter
        // nach der Transition über oder unter dem echten Boden landet.
        float groundY = startPos.y - targetSize * 0.5f; // Fallback, falls kein Treffer gefunden wird
        if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, groundRayMaxDistance * 2f, groundLayerMask))
        {
            groundY = hit.point.y;
        }
        else
        {
            Debug.LogWarning("ScaleController: Kein Ground-Treffer beim Raycast gefunden, verwende Fallback-Y.");
        }

        float targetY = groundY + targetSize * 0.5f; // Annahme: Pivot des Charakters liegt in der Mitte
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);

        float startFOV = playerCamera.fieldOfView;
        float targetFOV = FOVs[(int)category];

        // Rigidbody während der Transition kinematisch schalten:
        // dadurch ignoriert er Gravitation und Kollisions-Antworten,
        // sodass unser manuelles Positionssetzen nicht mit der Physik "kämpft".
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            //playerRigidbody.isKinematic = true;
        }

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            playerObject.transform.localScale = Vector3.Lerp(startScale, targetScale, smooth);
            SetPlayerPosition(Vector3.Lerp(startPos, targetPos, smooth));
            playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, smooth);

            yield return null;
        }

        // Exakte Endwerte setzen
        playerObject.transform.localScale = targetScale;
        SetPlayerPosition(targetPos);
        playerCamera.fieldOfView = targetFOV;

        playerMovement.moveSpeed = movementSpeeds[(int)category];

        // Rigidbody wieder in den Ursprungszustand versetzen und Geschwindigkeit
        // zurücksetzen, damit keine "alte" Fallgeschwindigkeit plötzlich wieder greift.
        if (playerRigidbody != null)
        {
            //playerRigidbody.isKinematic = false;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        cameraTransitionCoroutine = null;
    }

    // Setzt die Position sowohl am Transform als auch (falls vorhanden) am Rigidbody,
    // damit Physik und Darstellung während der Skalierung synchron bleiben.
    private void SetPlayerPosition(Vector3 position)
    {
        if (playerRigidbody != null)
            playerRigidbody.position = position;

        playerObject.transform.position = position;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}