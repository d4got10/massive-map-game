using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearPlayerPrefs : MonoBehaviour
{
    public void OnClick_Clear()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }
}
