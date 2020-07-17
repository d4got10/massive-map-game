using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField] private Player _player;
    [Space]
    [SerializeField] private InputField _nameField;
    [SerializeField] private InputField _rField;
    [SerializeField] private InputField _gField;
    [SerializeField] private InputField _bField;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("CharacterCreated"))
        {
            Destroy(gameObject);
        }
    }

    public void OnClick_Apply()
    {
        int r = 0;
        int g = 0;
        int b = 0;

        if(_rField.text.Length > 0)
            r = Mathf.Clamp(Convert.ToInt32(_rField.text), 0, 256);
        if (_gField.text.Length > 0)
            g = Mathf.Clamp(Convert.ToInt32(_gField.text), 0, 256);
        if (_bField.text.Length > 0)
            b = Mathf.Clamp(Convert.ToInt32(_bField.text), 0, 256);
        _player.Color = new Color(r/256f, g/256f, b/256f);

        _player.Name = _nameField.text;

        PlayerPrefs.SetInt("CharacterCreated", 1);

        Destroy(gameObject);
    }
}
