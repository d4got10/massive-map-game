using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OtherPlayer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private TextMeshProUGUI _text;

    public int ID;
    public int X;
    public int Y;
    public string Name;
    public Color Color;

    public bool PositionUpdate;
    public bool Setup;

    public void Update()
    {
        if (PositionUpdate)
        {
            PositionUpdate = false;
            transform.position = new Vector3(X, Y);
        }
        if (Setup)
        {
            Setup = false;
            _renderer.color = Color;
            _text.text = Name;
        }
    }
}
