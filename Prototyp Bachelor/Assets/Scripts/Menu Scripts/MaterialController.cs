using UnityEngine;
using System;
using System.Collections;

public enum MaterialParameter
{
    STUMP,
    LEAVES,
    STONES,
    GRASS,
    LANDSCAPE
}

[ExecuteInEditMode]
public class MaterialController : MonoBehaviour
{

    [SerializeField] private MaterialList[] materialList;

    public Material GetGenreMaterial(MaterialParameter taggedGameObject, Categories category)
    {
        return materialList[(int)taggedGameObject].Materials[(int)category];
    }

#if UNITY_EDITOR
    public void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(MaterialParameter));
        Array.Resize(ref materialList, names.Length);
        for (int i = 0; i < materialList.Length; i++)
        {
            materialList[i].name = names[i];
        }
    }
#endif
}

[Serializable]
public struct MaterialList
{
    public Material[] Materials { get => parameterMaterials; }
    [HideInInspector] public string name;
    [SerializeField] private Material[] parameterMaterials;
}

public struct ParameterMaterials
{
    string name;
    Material parameterMaterial;
}