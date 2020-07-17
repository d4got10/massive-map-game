using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    public int ID;
    public int X;
    public int Y;

    public void Update()
    {
        if (transform.position != new Vector3(X, Y))
            transform.position = new Vector3(X, Y);
    }
}
