using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseApp : MonoBehaviour
{
    [SerializeField] private GameObject _content;

    private bool _active;

    private void Awake()
    {
        Client.OnDisconnect += () =>
        {
            _active = true;
        };
    }

    public void Update()
    {
        if (_active)
        {
            _active = false;
            _content.SetActive(true);
        }
    }

    public void OnClick_CloseApp()
    {
        Invoke(nameof(Close), 3);
    }

    private void Close()
    {
        Application.Quit();
    }
}
