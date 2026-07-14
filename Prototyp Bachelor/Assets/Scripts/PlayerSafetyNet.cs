using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerSafetyNet : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    [Tooltip("Y-Position, auf die der Spieler nach dem Auffangen gesetzt wird.")]
    [SerializeField] private float resetHeight = 1f;

    private Rigidbody playerRigidbody;

    private void Awake()
    {
        if (playerObject != null)
            playerRigidbody = playerObject.GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerObject == null)
            return;

        bool isPlayer = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject == playerObject
            : other.gameObject == playerObject;

        if (!isPlayer)
            return;

        ResetPlayerPosition();
    }

    private void ResetPlayerPosition()
    {
        Vector3 targetPosition = new Vector3(
            playerObject.transform.position.x,
            resetHeight,
            playerObject.transform.position.z);

        if (playerRigidbody != null)
        {
            // Fallgeschwindigkeit zurücksetzen, sonst fällt der Spieler
            // direkt wieder durch die Plane.
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.position = targetPosition;
        }

        playerObject.transform.position = targetPosition;
    }
}