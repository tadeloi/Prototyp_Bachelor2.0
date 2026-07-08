using UnityEngine;

public class PlayerSettings : MonoBehaviour
{

    [HideInInspector] public float[] movementSpeeds;

    [HideInInspector] public float[] playerSizes;

    [Header("Empty")]
    public float emptyMovementSpeed = 70f;
    public float emptyPlayerSize = 1f;


    [Header("Horror")]
    public float horrorMovementSpeed = 70f;
    public float horrorPlayerSize = 1f;

    [Header("Cozy")]
    public float cozyMovementSpeed = 70f;
    public float cozyPlayerSize = 1f;

    [Header("Fantasy")]
    public float fantasyMovementSpeed = 70f;
    public float fantasyPlayerSize = 1f;

    [Header("SciFi")]
    public float sciFiMovementSpeed = 70f;
    public float sciFiPlayerSize = 1f;

    [Header("Logik")]
    public float logikMovementSpeed = 70f;
    public float logikPlayerSize = 1f;

    [Header("Retro")]
    public float retroMovementSpeed = 70f;
    public float retroPlayerSize = 1f;

    void Start()
    {
        movementSpeeds = new float[7];
        playerSizes = new float[7];
        for (int i = 0; i < movementSpeeds.Length; i++)
        {
            if (i == 0)
            {
                movementSpeeds[i] = emptyMovementSpeed;
                playerSizes[i] = emptyPlayerSize;
            }
            else if (i == 1)
            {
                movementSpeeds[i] = horrorMovementSpeed;
                playerSizes[i] = horrorPlayerSize;
            }
            else if (i == 2)
            {
                movementSpeeds[i] = cozyMovementSpeed;
                playerSizes[i] = cozyPlayerSize;
            }
            else if (i == 3)
            {
                movementSpeeds[i] = fantasyMovementSpeed;
                playerSizes[i] = fantasyPlayerSize;
            }
            else if (i == 4)
            {
                movementSpeeds[i] = sciFiMovementSpeed;
                playerSizes[i] = sciFiPlayerSize;
            }
            else if (i == 5)
            {
                movementSpeeds[i] = logikMovementSpeed;
                playerSizes[i] = logikPlayerSize;
            }
            else if (i == 6)
            {
                movementSpeeds[i] = retroMovementSpeed;
                playerSizes[i] = retroPlayerSize;
            }
            else
            {
                Debug.LogError("WHAT THE HELL IS HAPPENING IN PLAYERSETTINGS");
                return;
            }
            Debug.Log("Set new PlayerSize of Category: " + (Categories)i + " to value: " + playerSizes[i]);
        }
    }

    public void UpdateArray()
    {
        Debug.Log("Updating all Playersets");
        for (int i = 0; i < movementSpeeds.Length; i++)
        {
            if (i == 0)
            {
                movementSpeeds[i] = emptyMovementSpeed;
                playerSizes[i] = emptyPlayerSize;
            }
            else if (i == 1)
            {
                movementSpeeds[i] = horrorMovementSpeed;
                playerSizes[i] = horrorPlayerSize;
            }
            else if (i == 2)
            {
                movementSpeeds[i] = cozyMovementSpeed;
                playerSizes[i] = cozyPlayerSize;
            }
            else if (i == 3)
            {
                movementSpeeds[i] = fantasyMovementSpeed;
                playerSizes[i] = fantasyPlayerSize;
            }
            else if (i == 4)
            {
                movementSpeeds[i] = sciFiMovementSpeed;
                playerSizes[i] = sciFiPlayerSize;
            }
            else if (i == 5)
            {
                movementSpeeds[i] = logikMovementSpeed;
                playerSizes[i] = logikPlayerSize;
            }
            else if (i == 6)
            {
                movementSpeeds[i] = retroMovementSpeed;
                playerSizes[i] = retroPlayerSize;
            }
            else
            {
                Debug.LogError("WHAT THE HELL IS HAPPENING IN PLAYERSETTINGS");
                return;
            }
            Debug.Log("Set new PlayerSize of Category: " + (Categories)i + " to value: " + playerSizes[i]);
        }
    }
}