using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using TMPro;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;


public class ParameterSelection : MonoBehaviour
{

    [Header("Input Repeat Settings")]
    [SerializeField] private float initialRepeatDelay = 0.4f;
    [SerializeField] private float repeatRate = 0.15f;

    private int lastVerticalDirection = 0;
    private float nextVerticalInputTime = 0f;

    private int lastHorizontalDirection = 0;
    private float nextHorizontalInputTime = 0f;

    private int lastGenreDirection = 0;
    private float nextGenreInputTime = 0f;

    private enum NavigationMode { Parameters, Presets }
    private NavigationMode navigationMode = NavigationMode.Parameters;
    private int presetIndex = 0;

    [SerializeField] private GameObject[] presetButtons = new GameObject[7];

    [SerializeField] public GameObject[] choosableParameters = new GameObject[6];
    private enum RowElement { Title = 0, Toggle = 1, Category = 2 }
    private RowElement currentElement = RowElement.Title;
    [SerializeField] private Material[] categoryColors = new Material[7];

    [SerializeField] private Material[] treeStumpMaterials = new Material[7];
    [SerializeField] private Material[] treeLeavesMaterials = new Material[7];
    [SerializeField] private Material[] stonesMaterials = new Material[7];
    [SerializeField] private GameObject[] cameras = new GameObject[7];
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private PlayerSettings playerSettings;
    [SerializeField] private GameObject[] particleList = new GameObject[7];
    [SerializeField] private GameObject parameterCanvas;

    private ParameterRenderer[] parameterRenderSettings = new ParameterRenderer[7];
    private VolumeStorage[] parameterVolumes = new VolumeStorage[7];

    private ParameterColumn[] columns;
    private GameObject currentOption;

    [SerializeField] private SpriteControllerScript spriteController;

    [Header("Inputs")]
    public InputSystem_Actions userUIInput;
    private InputAction vertical;
    private InputAction horizontal;
    private InputAction select;
    private InputAction selectGenre;
    private InputAction closeMenu;


    private int currentIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerObject.GetComponent<PlayerMovement>().enabled = false;

        currentOption = choosableParameters[0];
        SetHighlight(currentOption, currentElement, true);
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

    void Update()
    {
        ProcessRepeatingInput(vertical, true, ref lastVerticalDirection, ref nextVerticalInputTime, DoVerticalMovement);
        ProcessRepeatingInput(horizontal, false, ref lastHorizontalDirection, ref nextHorizontalInputTime, DoHorizontalMovement);
        ProcessRepeatingInput(selectGenre, false, ref lastGenreDirection, ref nextGenreInputTime, DoGenreMovement);
    }

    void ProcessRepeatingInput(InputAction action, bool useYAxis, ref int lastDirection, ref float nextInputTime, Action<int> onTrigger)
    {
        Vector2 input = action.ReadValue<Vector2>();
        float axisValue = useYAxis ? input.y : input.x;

        int direction = 0;
        if (useYAxis)
        {
            if (axisValue > 0.5f) direction = -1;
            else if (axisValue < -0.5f) direction = 1;
        }
        else
        {
            if (axisValue > 0.5f) direction = 1;
            else if (axisValue < -0.5f) direction = -1;
        }

        if (direction == 0)
        {
            lastDirection = 0; // Stick ist neutral -> Repeat-Zustand zurücksetzen
            return;
        }

        if (direction != lastDirection)
        {
            // Neue Auslenkung (oder Richtungswechsel) -> sofort 1 Schritt
            onTrigger(direction);
            lastDirection = direction;
            nextInputTime = Time.time + initialRepeatDelay;
        }
        else if (Time.time >= nextInputTime)
        {
            // Stick wird weiter in dieselbe Richtung gehalten -> Repeat
            onTrigger(direction);
            nextInputTime = Time.time + repeatRate;
        }
    }

    void DoVerticalMovement(int direction)
    {
        if (navigationMode == NavigationMode.Presets)
        {
            if (direction == 1)
            {
                SetPresetHighlight(presetIndex, false);
                navigationMode = NavigationMode.Parameters;
                SetHighlight(choosableParameters[currentIndex], currentElement, true);
            }
            return;
        }

        bool atTop = currentIndex == 0 && direction == -1;
        if (atTop)
        {
            SetHighlight(choosableParameters[currentIndex], currentElement, false);
            navigationMode = NavigationMode.Presets;
            SetPresetHighlight(presetIndex, true);
            return;
        }

        Debug.Log("Trying to set Highlight OFF for: " + choosableParameters[currentIndex].ToString());
        SetHighlight(choosableParameters[currentIndex], currentElement, false);

        currentIndex = (currentIndex + direction + choosableParameters.Length) % choosableParameters.Length;
        currentOption = choosableParameters[currentIndex];
        Debug.Log("Setting Highlight ON for: " + choosableParameters[currentIndex].ToString());
        SetHighlight(currentOption, currentElement, true);
    }

    void DoHorizontalMovement(int direction)
    {
        Debug.Log("Moving in horizontal" + direction);
        if (navigationMode == NavigationMode.Presets)
        {
            SetPresetHighlight(presetIndex, false);
            presetIndex = (presetIndex + direction + presetButtons.Length) % presetButtons.Length;
            SetPresetHighlight(presetIndex, true);

        }
        else
        {
            if (navigationMode != NavigationMode.Parameters) return;

            int newElement = Mathf.Clamp((int)currentElement + direction, 0, 2);
            Debug.Log("NewElement: " + (RowElement)newElement + "OldElement: " + currentElement);
            if (newElement == (int)currentElement) return;

            Debug.Log("Trying to set Highlight OFF for: " + currentOption.ToString());
            SetHighlight(currentOption, currentElement, false);
            currentElement = (RowElement)newElement;
            Debug.Log("Trying to set Highlight ON for: " + currentOption.ToString());
            SetHighlight(currentOption, currentElement, true);
        }
    }

    void DoGenreMovement(int direction)
    {
        if (navigationMode != NavigationMode.Parameters) return;
        if (currentElement != RowElement.Category) return;

        ChangeCategory(direction);
    }

    void SetHighlight(GameObject row, RowElement element, bool highlighted)
    {
        //differentiate between TitleImage, Toggle and Parameter selection
        if ((int)element == 0)
        {
            Transform titleImage = row.transform.Find("TitleImage");
            Selectable selectable = titleImage.GetComponent<Selectable>();
            if (highlighted)
            {
                Debug.Log("Highlighting TitleImage");
                selectable.OnPointerEnter(null);
            }
            else
            {
                Debug.Log("Dislighting TitleImage");
                selectable.OnPointerExit(null);
            }
        }
        else if ((int)element == 1)
        {
            if (row.name != "Sound")
            {
                Transform toggleObject = row.transform.Find("Toggle");
                UnityEngine.UI.Toggle selectable = toggleObject.GetComponent<UnityEngine.UI.Toggle>();
                UpdateToggleSprites(selectable, selectable.isOn);
                if (highlighted)
                {
                    selectable.OnPointerEnter(null);
                }
                else
                {
                    selectable.OnPointerExit(null);
                }
            }

        }
        else if ((int)element == 2)
        {
            Transform bg = row.transform.Find("Background");
            Transform parameter = row.transform.Find("Parameter");
            if (bg == null || parameter == null)
            {
                Debug.LogError("No Background GameObject found");
                return;
            }

            Selectable selectable = bg.GetComponent<Selectable>();
            if (selectable == null)
            {
                Debug.LogError("No selectable found");
                return;
            }

            if (highlighted)
            {
                selectable.OnPointerEnter(null);
                parameter.GetComponent<TextMeshProUGUI>().color = Color.black;
            }
            else
            {
                selectable.OnPointerExit(null);
                parameter.GetComponent<TextMeshProUGUI>().color = Color.white;
            }
        }
    }

    void SetPresetHighlight(int index, bool highlighted)
    {
        if (index < 0 || index >= presetButtons.Length) return;
        Selectable selectable = presetButtons[index].GetComponentInChildren<Selectable>();
        TextMeshProUGUI tmpText = presetButtons[index].GetComponentInChildren<TextMeshProUGUI>();
        if (selectable == null) return;
        if (tmpText == null) return;
        if (highlighted)
        {
            selectable.OnPointerEnter(null);
            tmpText.color = Color.black;
        }
        else
        {
            selectable.OnPointerExit(null);
            tmpText.color = Color.white;
        }
    }

    /* Saving old Highlight Code
    void FitBackgroundToElement(RectTransform bgRect, RectTransform targetRect)
    {
        if (bgRect == null || targetRect == null) return;

        // Background und Ziel-Element liegen im selben Parent, daher reicht das direkte Kopieren
        bgRect.anchorMin = targetRect.anchorMin;
        bgRect.anchorMax = targetRect.anchorMax;
        bgRect.pivot = targetRect.pivot;
        bgRect.anchoredPosition = targetRect.anchoredPosition;
        bgRect.sizeDelta = targetRect.sizeDelta;
    }
    


    Transform GetElementTransform(GameObject row, RowElement element)
    {
        string childName = element switch
        {
            RowElement.Title => "Text",
            RowElement.Toggle => "Toggle",
            RowElement.Category => "Parameter",
            _ => null
        };
        if (childName == null) return null;

        Transform t = row.transform.Find(childName);
        if (t == null && element == RowElement.Toggle)
            t = row.transform.Find("State"); // Fallback für die Sound-Zeile

        return t;
    }
    */

    void Awake()
    {
        userUIInput = new InputSystem_Actions();
    }

    void OnEnable()
    {
        vertical = userUIInput.UI.NavigateVertical;
        vertical.Enable();

        horizontal = userUIInput.UI.NavigateHorizontal;
        horizontal.Enable();

        select = userUIInput.UI.Submit;
        select.Enable();
        select.performed += Select;

        selectGenre = userUIInput.UI.SelectGenre;
        selectGenre.Enable();

        closeMenu = userUIInput.UI.Explore;
        closeMenu.Enable();
        closeMenu.performed += ExplorationMode;
    }

    void OnDisable()
    {
        vertical.Disable();
        horizontal.Disable();
        select.Disable();
        closeMenu.Disable();
    }

    void ChangeCategory(int direction)
    {
        ParameterColumn currentColumn = FindCurrentSelection(currentOption.name);
        if (currentColumn == null) return;
        if (direction > 0)
            currentColumn.SetNextCategory();
        else
            currentColumn.SetPreviousCategory();
        // Text-GameObject als Kind suchen
        Transform textTransform = currentOption.transform.Find("Parameter");
        if (textTransform == null) return;
        TMP_Text label = textTransform.GetComponent<TMP_Text>();
        if (label == null) return;
        label.text = currentColumn.chosenCategory.ToString();
        if (label.text == "SCIFI")
        {
            label.text = "SCI-FI";
        }

        if (currentColumn.IsActive())
            ApplyToWorld(currentColumn, currentColumn.chosenCategory);
        Debug.Log($"Horizontal Movement: {currentOption.name} → {currentColumn.chosenCategory}");
    }

    void ApplyToWorld(ParameterColumn columnName, Categories category)
    {
        GameObject world = GameObject.Find("Landscape");
        if (world == null) return;
        if (columnName.name == "Color")
        {
            Material selectedColor = categoryColors[(int)category];
            foreach (MeshRenderer renderer in world.GetComponentsInChildren<MeshRenderer>())
                renderer.material.color = selectedColor.color;
        }
        else if (columnName.name == "Material")
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
        else if (columnName.name == "Light")
        {

        }
        else if (columnName.name == "Sound")
        {
            SoundManager.FadeOutAndStop();

            switch (columnName.GetState())
            {
                case 1:
                    SoundManager.PlaySoundLooped(category, SoundType.MUSIC, 0.5f);
                    break;
                case 2:
                    SoundManager.PlaySoundLooped(category, SoundType.BACKGROUND);
                    break;
                case 3:
                    SoundManager.PlaySoundLooped(category, SoundType.MUSIC, 0.5f);
                    SoundManager.PlaySoundLooped(category, SoundType.BACKGROUND);
                    break;
                case 0:
                default:
                    break;
            }

            UpdateSoundIcon(columnName);
        }
        else if (columnName.name == "Scale")
        {
            if (cameraTransitionCoroutine != null)
                StopCoroutine(cameraTransitionCoroutine);

            cameraTransitionCoroutine = StartCoroutine(TransitionPlayer(category));
        }
        else if (columnName.name == "VFX")
        {
            if (vfxTransitionCoroutine != null)
                StopCoroutine(vfxTransitionCoroutine);

            vfxTransitionCoroutine = StartCoroutine(TransitionVFX(category));

            //ApplyRenderSettings(category);
        }
    }

    void UpdateSoundIcon(ParameterColumn columnName)
    {
        int index = Array.IndexOf(columns, columnName);
        if (index < 0 || index >= choosableParameters.Length) return;

        Transform stateTransform = choosableParameters[index].transform.Find("State");
        if (stateTransform == null) return;

        RawImage icon = stateTransform.GetComponent<RawImage>();
        SoundSpriteSaver spriteSaver = stateTransform.GetComponent<SoundSpriteSaver>();
        if (icon == null || spriteSaver == null) return;

        switch (columnName.GetState())
        {
            case 1:
                icon.texture = spriteSaver.musicSprite.texture;
                break;
            case 2:
                icon.texture = spriteSaver.backgroundSprite.texture;
                break;
            case 3:
                icon.texture = spriteSaver.bothSprite.texture;
                break;
            case 0:
            default:
                icon.texture = spriteSaver.offSprite.texture;
                break;
        }
    }

    void Select(InputAction.CallbackContext context)
    {
        if (navigationMode == NavigationMode.Presets)
        {
            ApplyPreset((Categories)presetIndex);
            return;
        }

        if (currentElement != RowElement.Toggle)
            return; // Titel & Kategorie reagieren nicht auf Submit

        void ApplyPreset(Categories category)
        {
            string[] columnNames = { "Color", "Material", "Light", "Sound", "Scale", "VFX" };

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i].chosenCategory = category;
                columns[i].SetActive();

                // Toggle-UI aktualisieren
                if (columns[i].name == "Sound")
                {
                    UpdateSoundIcon(columns[i]);
                }
                else
                {
                    choosableParameters[i].GetComponentInChildren<UnityEngine.UI.Toggle>().isOn = true;
                }

                // Label aktualisieren
                Transform textTransform = choosableParameters[i].transform.Find("Parameter");
                if (textTransform != null)
                {
                    TMP_Text label = textTransform.GetComponent<TMP_Text>();
                    if (label != null)
                        label.text = "<" + category.ToString() + ">";
                }

                // Auf die Welt anwenden
                ApplyToWorld(columns[i], category);
            }
        }

        ParameterColumn currentColumn = FindCurrentSelection(currentOption.name);
        currentColumn.ToggleParameter();
        UnityEngine.UI.Toggle currentToggle = currentOption.GetComponentInChildren<UnityEngine.UI.Toggle>();
        if (currentToggle != null)
            currentToggle.isOn = currentColumn.IsActive();
        if (!currentColumn.IsActive())
        {
            UpdateToggleSprites(currentToggle, false);
            ApplyToWorld(currentColumn, Categories.LEER);
        }
        else
        {
            UpdateToggleSprites(currentToggle, true);
            ApplyToWorld(currentColumn, currentColumn.chosenCategory);
        }

    }

    private void UpdateToggleSprites(UnityEngine.UI.Toggle optionToggle, bool isToggled)
    {
        if (optionToggle == null)
        {
            Debug.LogError("Provided Toggle was null.");
            return;
        }
        else
        {
            if (isToggled)
            {
                SpriteState tempState = optionToggle.spriteState;
                tempState.highlightedSprite = spriteController.selected_toggle_ON;
                optionToggle.spriteState = tempState;
            }
            else
            {
                SpriteState tempState = optionToggle.spriteState;
                tempState.highlightedSprite = spriteController.selected_toggle_OFF;
                optionToggle.spriteState = tempState;
            }
        }
    }

    public void ToggleSetting()
    {
        Debug.Log("Toggling...");
        FindCurrentSelection(currentOption.name).ToggleParameter();
        currentOption.GetComponentInChildren<UnityEngine.UI.Toggle>().isOn = FindCurrentSelection(currentOption.name).IsActive();
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

    private Coroutine vfxTransitionCoroutine;
    private List<GameObject> activeParticles = new List<GameObject>();

    private List<Transform> GetAllTreeTops()
    {
        List<Transform> treeTops = new List<Transform>();
        GameObject world = GameObject.Find("Landscape");
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
        if (category != Categories.LEER)
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

    public void ExplorationMode(InputAction.CallbackContext context)
    {
        playerObject.GetComponent<PlayerMovement>().enabled = true;
        parameterCanvas.SetActive(false);
        OnDisable();
    }
}

class ParameterColumn
{
    public int state = 0;
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

    public void ToggleParameter()
    {
        if (this.name == "Sound")
            state = (state + 1) % 4;
        else
            state = (state + 1) % 2;
    }

    public bool IsActive()
    {
        if (state == 0)
            return false;
        else
            return true;
    }

    public void SetActive()
    {
        if (this.name == "Sound")
            this.state = 3;
        else
            this.state = 1;
    }

    public int GetState()
    {
        return this.state;
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