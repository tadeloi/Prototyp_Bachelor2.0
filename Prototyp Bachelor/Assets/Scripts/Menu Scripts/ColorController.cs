using UnityEngine;
using System;
using System.Collections;

public enum ColorParameter
{
    TREE1STUMP,
    TREE1LEAVES,
    TREE2STUMP,
    TREE2LEAVES,
    TREE3STUMP,
    TREE3LEAVES,
    BUSH1STUMP,
    BUSH1LEAVES,
    BUSH2STUMP,
    BUSH2LEAVES,
    BUSH3STUMP,
    BUSH3LEAVES,
    STONES,
    GRASS,
    LANDSCAPE
}

[ExecuteInEditMode]
public class ColorController : MonoBehaviour
{

    [SerializeField] private ColorList[] colorList;

    public Material GetGenreMaterial(ColorParameter taggedGameObject, Categories category)
    {
        return colorList[(int)taggedGameObject].Materials[(int)category];
    }

#if UNITY_EDITOR
    public void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(ColorParameter));
        Array.Resize(ref colorList, names.Length);
        for (int i = 0; i < colorList.Length; i++)
        {
            colorList[i].name = names[i];
        }
    }
#endif
}

[Serializable]
public struct ColorList
{
    public Material[] Materials { get => parameterColors; }
    [HideInInspector] public string name;
    [SerializeField] private Material[] parameterColors;
}

public struct ParameterColors
{
    string name;
    Material parameterMaterial;
}