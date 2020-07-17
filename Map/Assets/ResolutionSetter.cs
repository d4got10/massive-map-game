using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionSetter : MonoBehaviour
{
    private void Start()
    {
        Screen.SetResolution(640, 480, false);
    }
}
