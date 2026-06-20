using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ParameterSelection : MonoBehaviour
{
    [SerializeField] public GameObject[] choosableParameters = new GameObject[6];
    [SerializeField] private Material[] categoryMaterials = new Material[7];
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
        columns[5] = new ParameterColumn("Shape");
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

    void OnDisable ()
    {
        vertical.Disable();
        horizontal.Disable();
        select.Disable();
    }

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
    }
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
        if (input.x > 0.5f)
            ChangeCategory(1);
        else if (input.x < -0.5f)
            ChangeCategory(-1);
    }
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

    /*if (currentOption.name == "Color")
    {
        Material selectedMaterial = categoryMaterials[(int)currentColumn.chosenCategory];
        GameObject world = GameObject.Find("Landschaft_ForUnity");
        if (world == null) return;
        foreach (MeshRenderer renderer in world.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = selectedMaterial;
        }
    }*/
    }

    void ApplyToWorld(string columnName, Categories category)
    {
    GameObject world = GameObject.Find("Landschaft_ForUnity");
    if (world == null) return;
    if (columnName == "Color")
    {
        Material selectedMaterial = categoryMaterials[(int)category];
        foreach (MeshRenderer renderer in world.GetComponentsInChildren<MeshRenderer>())
            renderer.material = selectedMaterial;
    }
    else if (columnName == "Material")
    {
        // Material-Logik analog, sobald du sie brauchst
    }
    else if (columnName == "Light")
    {
            
    }
    else if (columnName == "Sound")
    {
        SoundManager.Stop();

        SoundManager.PlaySoundLooped(category, SoundType.MUSIC);
    }
    else if (columnName == "Scale")
    {
            
    }
    else if (columnName == "Shape")
    {
            
    }
}

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
        for(int i = 0; i < columns.Length; i++)
        {
            if(columns[i].name == column)
                return columns[i];
        }
        Debug.LogError("Selected Column not found.");
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if((int)chosenCategory >= 6)
            chosenCategory = 0;
        else
            chosenCategory++;
    }

    public void SetPreviousCategory()
    {
        if((int)chosenCategory <= 0)
            chosenCategory = (Categories)6;
        else
            chosenCategory--;
    }

    public void Toggle()
    {
        isActive = !isActive;
    }
}