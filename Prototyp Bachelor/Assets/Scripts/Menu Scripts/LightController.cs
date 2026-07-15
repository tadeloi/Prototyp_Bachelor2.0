using Unity.VisualScripting;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] public GameObject directionalLight;
    [SerializeField] public GameObject[] parameterDirectionalLights;
    [SerializeField] public Material[] parameterSkyboxes;

    public GameObject flashlight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void UpdateDirectionalLight(Categories category)
    {
        if (category == Categories.HORROR)
        {
            flashlight.SetActive(true);
        }
        else
        {
            flashlight.SetActive(false);
        }
        Destroy(directionalLight);
        directionalLight = Instantiate(parameterDirectionalLights[(int)category]);
        RenderSettings.skybox = parameterSkyboxes[(int)category];
    }
}
