using System;
using UnityEngine;

public enum Categories
{
    Empty = 0,
    Horror = 1,
    Cozy = 2,
    Fantasy = 3,
    SciFi = 4,
    Logik = 5,
    Retro = 6

}

public class ParameterMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*public Categories GetNextMenu(Categories currentOption)
    {
        int lastIndex = Enum.GetNames(typeof(Categories)).Length - 1;

        if ((int)currentOption > lastIndex)
            return Categories.Empty;
        else
            return (currentOption+1);
    }*/
}
