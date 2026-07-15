using System;
using UnityEngine;

public enum Categories
{
    LEER = 0,
    HORROR = 1,
    COZY = 2,
    FANTASY = 3,
    SCIFI = 4,
    LOGIK = 5,
    RETRO = 6

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
