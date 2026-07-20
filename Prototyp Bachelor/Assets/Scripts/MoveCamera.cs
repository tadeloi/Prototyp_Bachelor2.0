using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

    void OnEnable()
    {
        transform.position = cameraPosition.position;
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = cameraPosition.position;
    }
}