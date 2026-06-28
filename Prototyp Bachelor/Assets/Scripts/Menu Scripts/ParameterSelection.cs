using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using TMPro;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;


public class ParameterSelection : MonoBehaviour
{
    private enum NavigationMode { Parameters, Presets }
    private NavigationMode navigationMode = NavigationMode.Parameters;
    private int presetIndex = 0;

    [SerializeField] private GameObject[] presetButtons = new GameObject[7];

    [SerializeField] public GameObject[] choosableParameters = new GameObject[6];
    [SerializeField] private Material[] categoryColors = new Material[7];

    [SerializeField] private Material[] treeStumpMaterials = new Material[7];
    [SerializeField] private Material[] treeLeavesMaterials = new Material[7];
    [SerializeField] private Material[] stonesMaterials = new Material[7];
    [SerializeField] private GameObject[] cameras = new GameObject[7];
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private PlayerSettings playerSettings;
    [SerializeField] private GameObject[] particleList = new GameObject[7];



    private ParameterRenderer[] parameterRenderSettings = new ParameterRenderer[7];
    private VolumeStorage[] parameterVolumes = new VolumeStorage[7];

    private ParameterColumn[] columns;
    private GameObject currentOption;

    public InputSystem_Actions userUIInput;
    private InputAction vertical;
    private InputAction horizontal;
    private InputAction select;


    private int currentIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentOption = choosableParameters[0];
        SetHighlight(currentOption, true);
        columns = new ParameterColumn[6];

        columns[0] = new ParameterColumn("Color");
        columns[1] = new ParameterColumn("Material");
        columns[2] = new ParameterColumn("Light");
        columns[3] = new ParameterColumn("Sound");
        columns[4] = new ParameterColumn("Scale");
        columns[5] = new ParameterColumn("VFX");


        for (int i = 0; i < parameterRenderSettings.Length; i++)
        {
            parameterVolumes[i] = new VolumeStorage();

            //Empty
            if (i == 0)
            {
                parameterVolumes[i].renderSettings = new RenderSettingsStorage(false, 5f, 5f, (FogMode)1, new Color(1, 1, 1, 1));
            }
            //Horror
            else if (i == 1)
            {
                parameterVolumes[i].renderSettings = new RenderSettingsStorage(true, 0.1f, 1.8f, (FogMode)1, new Color(0.1f, 0.12f, 0.1f, 1f), 1f);
            }
            //Cozy
            else if (i == 2)
            {
                parameterVolumes[i].renderSettings = new RenderSettingsStorage(true, .8f, 6.0f, (FogMode)2, new Color(0.85f, 0.75f, 0.6f, 1f), 1f);
            }
            //Fantasy
            else if (i == 3)
            {
                parameterVolumes[i].renderSettings = new RenderSettingsStorage(true, .2f, 4.0f, (FogMode)2, new Color(0.4f, 0.15f, 0.6f, 1f), 1f);
            }
            //SciFi
            else if (i == 4)
            {
                parameterVolumes[i].renderSettings = new RenderSettingsStorage(true, .5f, 5.0f, (FogMode)3, new Color(0.05f, 0.15f, 0.35f, 1f), 1f);
            }
            //Logik
            else if (i == 5)
            {
                parameterVolumes[i].renderSettings = new RenderSettingsStorage(true, 1f, 8.0f, (FogMode)1, new Color(0.9f, 0.9f, 0.95f, 1f), 1f);
            }
            //Retro
            else if (i == 6)
            {
                parameterVolumes[i].renderSettings = new RenderSettingsStorage(true, .3f, 3.5f, (FogMode)2, new Color(0.7f, 0.4f, 0.1f, 1f), 1f);
            }
        }

        for (int i = 0; i < parameterVolumes.Length; i++)
        {
            parameterRenderSettings[(int)((Categories)i)] = new ParameterRenderer(((Categories)i).ToString());
            //Debug.Log(parameterRenderSettings[i].name);

            //Empty
            if (i == 0)
            {

            }
            //Horror
            else if (i == 1)
            {

            }
            //Cozy
            else if (i == 2)
            {

            }
            //Fantasy
            else if (i == 3)
            {

            }
            //SciFi
            else if (i == 4)
            {

            }
            //Logik
            else if (i == 5)
            {

            }
            //Retro
            else if (i == 6)
            {

            }
        }
        playerSettings.UpdateArray();
    }

    void Awake()
    {
        userUIInput = new InputSystem_Actions();
    }

    void OnEnable()
    {
        vertical = userUIInput.UI.NavigateVertical;
        vertical.Enable();
        vertical.performed += VerticalMovement;

        horizontal = userUIInput.UI.NavigateHorizontal;
        horizontal.Enable();
        horizontal.performed += HorizontalMovement;

        select = userUIInput.UI.Submit;
        select.Enable();
        select.performed += Select;
    }

    void OnDisable()
    {
        vertical.Disable();
        horizontal.Disable();
        select.Disable();
    }

    void VerticalMovement(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        int direction = 0;
        if (input.y > 0.5f) direction = -1;
        else if (input.y < -0.5f) direction = 1;
        if (direction == 0) return;

        if (navigationMode == NavigationMode.Presets)
        {
            // Von Presets nach unten → zurück zu Parametern
            if (direction == 1)
            {
                SetPresetHighlight(presetIndex, false);
                navigationMode = NavigationMode.Parameters;
                SetHighlight(choosableParameters[currentIndex], true);
            }
            return;
        }

        // NavigationMode.Parameters
        bool atTop = currentIndex == 0 && direction == -1;
        if (atTop)
        {
            // Rauf aus der Liste → in Preset-Zeile wechseln
            SetHighlight(choosableParameters[currentIndex], false);
            navigationMode = NavigationMode.Presets;
            SetPresetHighlight(presetIndex, true);
            return;
        }

        SetHighlight(choosableParameters[currentIndex], false);
        currentIndex = (currentIndex + direction + choosableParameters.Length) % choosableParameters.Length;
        currentOption = choosableParameters[currentIndex];
        SetHighlight(currentOption, true);
    }


    /*Saving old VerticalMovement in Case of Fallback
    void VerticalMovement(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        int direction = 0;
        if (input.y > 0.5f) direction = -1;       // hoch = vorheriger Eintrag
        else if (input.y < -0.5f) direction = 1;  // runter = nächster Eintrag
        if (direction == 0) return;
        // Altes Highlight zurücksetzen
        SetHighlight(choosableParameters[currentIndex], false);
        // Index aktualisieren mit Wrap-Around
        currentIndex = (currentIndex + direction + choosableParameters.Length) % choosableParameters.Length;
        currentOption = choosableParameters[currentIndex];
        // Neues Highlight setzen
        SetHighlight(currentOption, true);
        Debug.Log($"Vertical Movement: {currentOption.name} ausgewählt");
    }*/

    void SetHighlight(GameObject option, bool highlighted)
    {
        Transform bg = option.transform.Find("Background");
        if (bg == null) return;
        Selectable selectable = bg.GetComponent<Selectable>();
        Image image = bg.GetComponent<Image>();
        if (selectable == null) return;
        if (highlighted)
        {
            // Simuliert den Highlighted-Zustand nativ
            selectable.OnPointerEnter(null);
        }
        else
        {
            // Setzt zurück auf Normal
            selectable.OnPointerExit(null);
        }
    }

    void HorizontalMovement(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        int direction = input.x > 0.5f ? 1 : input.x < -0.5f ? -1 : 0;
        if (direction == 0) return;

        if (navigationMode == NavigationMode.Presets)
        {
            SetPresetHighlight(presetIndex, false);
            presetIndex = (presetIndex + direction + presetButtons.Length) % presetButtons.Length;
            SetPresetHighlight(presetIndex, true);
            return;
        }
        ChangeCategory(direction);
    }


    /*Saving old HorizontalMovement in Case of Fallback
    void HorizontalMovement(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input.x > 0.5f)
            ChangeCategory(1);
        else if (input.x < -0.5f)
            ChangeCategory(-1);
    }*/

    void ChangeCategory(int direction)
    {
        ParameterColumn currentColumn = FindCurrentSelection(currentOption.name);
        if (currentColumn == null) return;
        if (direction > 0)
            currentColumn.SetNextCategory();
        else
            currentColumn.SetPreviousCategory();
        // Text-GameObject direkt als Kind suchen statt über GameObject.Find
        Transform textTransform = currentOption.transform.Find("Parameter");
        if (textTransform == null) return;
        TMP_Text label = textTransform.GetComponent<TMP_Text>();
        if (label == null) return;
        label.text = "<" + currentColumn.chosenCategory.ToString() + ">";

        if (currentColumn.isActive)
            ApplyToWorld(currentOption.name, currentColumn.chosenCategory);
        Debug.Log($"Horizontal Movement: {currentOption.name} → {currentColumn.chosenCategory}");
    }

    void ApplyToWorld(string columnName, Categories category)
    {
        GameObject world = GameObject.Find("Landschaft_ForUnity");
        if (world == null) return;
        if (columnName == "Color")
        {
            Material selectedColor = categoryColors[(int)category];
            foreach (MeshRenderer renderer in world.GetComponentsInChildren<MeshRenderer>())
                renderer.material.color = selectedColor.color;
        }
        else if (columnName == "Material")
        {
            Material stumpMaterial = treeStumpMaterials[(int)category];
            Material leavesMaterial = treeLeavesMaterials[(int)category];
            Material stoneMaterial = stonesMaterials[(int)category];
            Color currentColor;
            string currentTag;
            foreach (MeshRenderer renderer in world.GetComponentsInChildren<MeshRenderer>())
            {
                currentTag = renderer.transform.gameObject.tag;
                currentColor = renderer.material.color;
                if (currentTag == "TreeTrunk")
                {
                    renderer.material = stumpMaterial;
                    renderer.material.color = currentColor;
                }
                else if (currentTag == "TreeTop")
                {
                    renderer.material = leavesMaterial;
                    renderer.material.color = currentColor;
                }
                else if (currentTag == "Stone")
                {
                    renderer.material = stoneMaterial;
                    renderer.material.color = currentColor;
                }

            }
        }
        else if (columnName == "Light")
        {

        }
        else if (columnName == "Sound")
        {
            //SoundManager.Stop();
            SoundManager.FadeOutAndStop();

            SoundManager.PlaySoundLooped(category, SoundType.MUSIC);
        }
        else if (columnName == "Scale")
        {
            if (cameraTransitionCoroutine != null)
                StopCoroutine(cameraTransitionCoroutine);

            cameraTransitionCoroutine = StartCoroutine(TransitionPlayer(category));

            /*Saving old Camera transition code
            GameObject targetCameraObj = cameras[(int)category];
            Camera targetCam = targetCameraObj.GetComponent<Camera>();
            Transform targetTransform = targetCameraObj.GetComponent<Transform>();

            if (targetCam == null) return;

            if (cameraTransitionCoroutine != null)
                StopCoroutine(cameraTransitionCoroutine);

            cameraTransitionCoroutine = StartCoroutine(TransitionCamera(targetCam, targetTransform));*/
        }
        else if (columnName == "VFX")
        {
            if (vfxTransitionCoroutine != null)
                StopCoroutine(vfxTransitionCoroutine);

            vfxTransitionCoroutine = StartCoroutine(TransitionVFX(category));

            //ApplyRenderSettings(category);
        }
    }

    void Select(InputAction.CallbackContext context)
    {
        if (navigationMode == NavigationMode.Presets)
        {
            ApplyPreset((Categories)presetIndex);
            return;
        }

        void ApplyPreset(Categories category)
        {
            string[] columnNames = { "Color", "Material", "Light", "Sound", "Scale", "VFX" };

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i].chosenCategory = category;
                columns[i].isActive = true;

                // Toggle-UI aktualisieren
                choosableParameters[i].GetComponentInChildren<Toggle>().isOn = true;

                // Label aktualisieren
                Transform textTransform = choosableParameters[i].transform.Find("Parameter");
                if (textTransform != null)
                {
                    TMP_Text label = textTransform.GetComponent<TMP_Text>();
                    if (label != null)
                        label.text = "<" + category.ToString() + ">";
                }

                // Auf die Welt anwenden
                ApplyToWorld(columnNames[i], category);
            }
        }

        ParameterColumn currentColumn = FindCurrentSelection(currentOption.name);
        currentColumn.Toggle();
        currentOption.GetComponentInChildren<Toggle>().isOn = currentColumn.isActive;
        if (!currentColumn.isActive)
            ApplyToWorld(currentOption.name, Categories.Empty);
        else
            ApplyToWorld(currentOption.name, currentColumn.chosenCategory);
    }

    /*Save old Select
    void Select(InputAction.CallbackContext context)
    {
        Debug.Log("Selection");
        ParameterColumn currentColumn = FindCurrentSelection(currentOption.name);
        currentColumn.Toggle();
        currentOption.GetComponentInChildren<Toggle>().isOn = currentColumn.isActive;
        if (!currentColumn.isActive)
        {
            // Toggle wurde ausgeschaltet → Empty anwenden
            ApplyToWorld(currentOption.name, Categories.Empty);
        }
        else
        {
            // Toggle wurde eingeschaltet → aktuelle Category anwenden
            ApplyToWorld(currentOption.name, currentColumn.chosenCategory);
        }
    }*/

    void SetPresetHighlight(int index, bool highlighted)
    {
        if (index < 0 || index >= presetButtons.Length) return;
        Selectable selectable = presetButtons[index].GetComponentInChildren<Selectable>();
        if (selectable == null) return;
        if (highlighted)
            selectable.OnPointerEnter(null);
        else
            selectable.OnPointerExit(null);
    }

    public void ToggleSetting()
    {
        Debug.Log("Toggling...");
        FindCurrentSelection(currentOption.name).Toggle();
        currentOption.GetComponentInChildren<Toggle>().isOn = FindCurrentSelection(currentOption.name).isActive;
    }


    public void ChangeParameter()
    {
        Debug.Log("Changing Parameter to next Item");
        ParameterColumn currentParameterColumn = FindCurrentSelection(currentOption.name);
        GameObject parameter = GameObject.Find(currentOption.name + "/Parameter");
        currentParameterColumn.SetNextCategory();
        parameter.GetComponent<TextMeshPro>().text = "<" + currentParameterColumn.name + ">";
    }

    private ParameterColumn FindCurrentSelection(string column)
    {
        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i].name == column)
                return columns[i];
        }
        Debug.LogError("Selected Column not found.");
        return null;
    }

    private Coroutine cameraTransitionCoroutine;

    IEnumerator TransitionPlayer(Categories category)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Debug.Log("Setting Scale to: " + category);
        for (int i = 0; i < playerSettings.playerSizes.Length; i++)
            Debug.Log((int)category);

        float targetSize = playerSettings.playerSizes[(int)category];
        float targetY = targetSize * 0.5f; //y anpassen damit der Spieler weiterhin auf dem Boden steht

        Vector3 startScale = playerObject.transform.localScale;
        Vector3 targetScale = new(targetSize, targetSize, targetSize);

        Vector3 startPos = playerObject.transform.position;
        Vector3 targetPos = new(startPos.x, targetY, startPos.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            playerObject.transform.localScale = Vector3.Lerp(startScale, targetScale, smooth);
            playerObject.transform.position = Vector3.Lerp(startPos, targetPos, smooth);

            yield return null;
        }

        // Exakte Endwerte setzen
        playerObject.transform.localScale = targetScale;
        playerObject.transform.position = targetPos;

        PlayerMovement pm = playerObject.GetComponent<PlayerMovement>();
        if (pm != null)
            pm.moveSpeed = playerSettings.movementSpeeds[(int)category];
    }


    /* Saving old Camera Transition
    IEnumerator TransitionCamera(Camera targetCamData, Transform targetTransform)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Camera mainCam = mainCamera.GetComponent<Camera>();
        Transform mainCamTransform = mainCamera.transform;

        // Startwerte sichern
        Vector3 startPos = mainCamTransform.position;
        Quaternion startRot = mainCamTransform.rotation;
        float startFOV = mainCam.fieldOfView;
        float startNear = mainCam.nearClipPlane;
        float startFar = mainCam.farClipPlane;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t); // sanftes Easing

            mainCamTransform.position = Vector3.Lerp(startPos, targetTransform.position, smooth);
            mainCamTransform.rotation = Quaternion.Slerp(startRot, targetTransform.rotation, smooth);
            mainCam.fieldOfView = Mathf.Lerp(startFOV, targetCamData.fieldOfView, smooth);
            mainCam.nearClipPlane = Mathf.Lerp(startNear, targetCamData.nearClipPlane, smooth);
            mainCam.farClipPlane = Mathf.Lerp(startFar, targetCamData.farClipPlane, smooth);

            yield return null;
        }

        // Exakte Endwerte setzen
        mainCamTransform.position = targetTransform.position;
        mainCamTransform.rotation = targetTransform.rotation;
        mainCam.fieldOfView = targetCamData.fieldOfView;
        mainCam.nearClipPlane = targetCamData.nearClipPlane;
        mainCam.farClipPlane = targetCamData.farClipPlane;
    } */

    private Coroutine vfxTransitionCoroutine;
    private List<GameObject> activeParticles = new List<GameObject>();

    private List<Transform> GetAllTreeTops()
    {
        List<Transform> treeTops = new List<Transform>();
        GameObject world = GameObject.Find("Landschaft_ForUnity");
        if (world == null) return treeTops;

        foreach (Transform child in world.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("TreeTop"))
                treeTops.Add(child);
        }
        return treeTops;
    }

    IEnumerator TransitionVFX(Categories category)
    {
        // Alte Partikel: Emission stoppen, nach Lifetime selbst zerstören
        foreach (GameObject ps in activeParticles)
        {
            if (ps == null) continue;
            ParticleSystem p = ps.GetComponent<ParticleSystem>();
            if (p != null)
            {
                var emission = p.emission;
                emission.enabled = false;
            }
            Destroy(ps, 20f); // Zerstört nach der maximalen Lifetime
        }
        activeParticles.Clear();

        // Neue Partikel sofort spawnen (wenn nicht Empty)
        if (category != Categories.Empty)
        {
            GameObject prefab = particleList[(int)category];
            if (prefab != null)
            {
                List<Transform> treeTops = GetAllTreeTops();
                foreach (Transform treeTop in treeTops)
                {
                    Vector3 center = treeTop.position;
                    Renderer rend = treeTop.GetComponent<Renderer>();
                    if (rend != null) center = rend.bounds.center;

                    GameObject spawnedPS = Instantiate(prefab, center, Quaternion.Euler(90f, 0f, 0f));
                    activeParticles.Add(spawnedPS);
                }
            }
        }
        ApplyRenderSettings(category);
        vfxTransitionCoroutine = null;
        yield break;
    }

    void ApplyRenderSettings(Categories category)
    {
        RenderSettingsStorage settings = parameterVolumes[(int)category].renderSettings;
        if (settings == null)
        {
            Debug.LogError("Fog settings not instantiated!");
            return;
        }
        RenderSettings.fog = settings.fog;
        RenderSettings.fogStartDistance = settings.fogStartDistance;
        RenderSettings.fogEndDistance = settings.fogEndDistance;
        RenderSettings.fogMode = settings.fogMode;
        RenderSettings.fogColor = settings.fogColor;
        RenderSettings.fogDensity = settings.fogDensity;
    }

}

class ParameterColumn
{
    public bool isActive = false;
    public string name;
    public Categories chosenCategory = 0;

    public ParameterColumn()
    {

    }

    public ParameterColumn(string name)
    {
        this.name = name;
    }

    public void SetNextCategory()
    {
        if ((int)chosenCategory >= 6)
            chosenCategory = 0;
        else
            chosenCategory++;
    }

    public void SetPreviousCategory()
    {
        if ((int)chosenCategory <= 0)
            chosenCategory = (Categories)6;
        else
            chosenCategory--;
    }

    public void Toggle()
    {
        isActive = !isActive;
    }
}

class ParameterRenderer
{

    public string name;
    public RenderSettingsStorage renderSettings;
    public ParameterRenderer(string name)
    {
        this.name = name;
    }


}

class VolumeStorage
{
    public RenderSettingsStorage renderSettings;

    public VolumeStorage()
    {

    }


}

class RenderSettingsStorage
{

    public RenderSettingsStorage()
    {

    }

    public RenderSettingsStorage(bool fog, float fogStartDistance, float fogEndDistance, FogMode fogMode, Color fogColor)
    {
        this.fog = fog;
        this.fogStartDistance = fogStartDistance;
        this.fogEndDistance = fogEndDistance;
        this.fogMode = fogMode;
        this.fogColor = fogColor;
    }
    public RenderSettingsStorage(bool fog, float fogStartDistance, float fogEndDistance, FogMode fogMode, Color fogColor, float fogDensity)
    {
        this.fog = fog;
        this.fogStartDistance = fogStartDistance;
        this.fogEndDistance = fogEndDistance;
        this.fogMode = fogMode;
        this.fogColor = fogColor;
        this.fogDensity = fogDensity;
    }

    public void setFog(bool fog, float fogStartDistance, float fogEndDistance, FogMode fogMode, Color fogColor, float fogDensity)
    {
        this.fog = fog;
        this.fogStartDistance = fogStartDistance;
        this.fogEndDistance = fogEndDistance;
        this.fogMode = fogMode;
        this.fogColor = fogColor;
        this.fogDensity = fogDensity;
    }
    //
    // Summary:
    //     Is fog enabled?
    public bool fog;

    //
    // Summary:
    //     The starting distance of linear fog.
    public float fogStartDistance;

    //
    // Summary:
    //     The ending distance of linear fog.
    public float fogEndDistance;

    //
    // Summary:
    //     Fog mode to use.
    public FogMode fogMode;
    //
    // Summary:
    //     The color of the fog.
    public Color fogColor;

    //
    // Summary:
    //     The density of the exponential fog.
    public float fogDensity;

    //
    // Summary:
    //     Ambient lighting coming from above.
    public Color ambientSkyColor;

    //
    // Summary:
    //     Ambient lighting coming from the sides.
    public Color ambientEquatorColor;

    //
    // Summary:
    //     Ambient lighting coming from below.
    /* public static Color ambientGroundColor
    {
        get
        {
            get_ambientGroundColor_Injected(out var ret);
            return ret;
        }
        set
        {
            set_ambientGroundColor_Injected(ref value);
        }
    }

    //
    // Summary:
    //     How much the light from the Ambient Source affects the Scene.
    public static extern float ambientIntensity
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        set;
    }

    //
    // Summary:
    //     Flat ambient lighting color.
    [NativeProperty("AmbientSkyColor")]
    public static Color ambientLight
    {
        get
        {
            get_ambientLight_Injected(out var ret);
            return ret;
        }
        set
        {
            set_ambientLight_Injected(ref value);
        }
    }

    //
    // Summary:
    //     The color used for the sun shadows in the Subtractive lightmode.
    public static Color subtractiveShadowColor
    {
        get
        {
            get_subtractiveShadowColor_Injected(out var ret);
            return ret;
        }
        set
        {
            set_subtractiveShadowColor_Injected(ref value);
        }
    }

    //
    // Summary:
    //     The global skybox to use.
    [NativeProperty("SkyboxMaterial")]
    public static Material skybox
    {
        get
        {
            return Unmarshal.UnmarshalUnityObject<Material>(get_skybox_Injected());
        }
        set
        {
            set_skybox_Injected(MarshalledUnityObject.Marshal(value));
        }
    }

    //
    // Summary:
    //     The light used by the procedural skybox.
    public static Light sun
    {
        get
        {
            return Unmarshal.UnmarshalUnityObject<Light>(get_sun_Injected());
        }
        set
        {
            set_sun_Injected(MarshalledUnityObject.Marshal(value));
        }
    }

    //
    // Summary:
    //     An ambient probe that captures environment lighting.
    public static SphericalHarmonicsL2 ambientProbe
    {
        [NativeMethod("GetFinalAmbientProbe")]
        get
        {
            get_ambientProbe_Injected(out var ret);
            return ret;
        }
        set
        {
            set_ambientProbe_Injected(ref value);
        }
    }

    //
    // Summary:
    //     Custom specular reflection cubemap.
    [Obsolete("RenderSettings.customReflection has been deprecated in favor of RenderSettings.customReflectionTexture.", false)]
    public static Cubemap customReflection
    {
        get
        {
            return (customReflectionTexture as Cubemap) ?? throw new ArgumentException("RenderSettings.customReflection is currently not referencing a cubemap.");
        }
        [NativeThrows]
        set
        {
            customReflectionTexture = value;
        }
    }

    [NativeProperty("CustomReflection")]
    public static Texture customReflectionTexture
    {
        get
        {
            return Unmarshal.UnmarshalUnityObject<Texture>(get_customReflectionTexture_Injected());
        }
        [NativeThrows]
        set
        {
            set_customReflectionTexture_Injected(MarshalledUnityObject.Marshal(value));
        }
    }

    //
    // Summary:
    //     How much the skybox / custom cubemap reflection affects the Scene.
    public static extern float reflectionIntensity
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        set;
    }

    //
    // Summary:
    //     The number of times a reflection includes other reflections.
    public static extern int reflectionBounces
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        set;
    }

    [NativeProperty("GeneratedSkyboxReflection")]
    internal static Cubemap defaultReflection => Unmarshal.UnmarshalUnityObject<Cubemap>(get_defaultReflection_Injected());

    //
    // Summary:
    //     Default reflection mode.
    public static extern DefaultReflectionMode defaultReflectionMode
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        set;
    }

    //
    // Summary:
    //     Cubemap resolution for default reflection.
    public static extern int defaultReflectionResolution
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        set;
    }

    //
    // Summary:
    //     Size of the Light halos.
    public static extern float haloStrength
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        set;
    }

    //
    // Summary:
    //     The intensity of all flares in the Scene.
    public static extern float flareStrength
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        set;
    }

    //
    // Summary:
    //     The fade speed of all flares in the Scene.
    public static extern float flareFadeSpeed
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        set;
    }

    [NativeProperty("DefaultSpotCookie")]
    internal static Texture2D spotCookieTexture
    {
        get
        {
            return Unmarshal.UnmarshalUnityObject<Texture2D>(get_spotCookieTexture_Injected());
        }
        set
        {
            set_spotCookieTexture_Injected(MarshalledUnityObject.Marshal(value));
        }
    }

    internal static Texture2D haloTexture
    {
        get
        {
            return Unmarshal.UnmarshalUnityObject<Texture2D>(get_haloTexture_Injected());
        }
        set
        {
            set_haloTexture_Injected(MarshalledUnityObject.Marshal(value));
        }
    }
    */
}