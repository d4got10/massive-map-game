using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    [SerializeField] private InputField _inputField;
    [SerializeField] private Player _player;

    public void OnClick_Connect()
    {
        _player.Begin(_inputField.text);
    }
}
