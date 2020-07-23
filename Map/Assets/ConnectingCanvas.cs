using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingCanvas : MonoBehaviour
{
    [SerializeField] private GameObject _content;

    private bool _connected = false;

    private void Awake()
    {
        Client.OnConnect += Connected;
    }

    private void OnDestroy()
    {
        Client.OnConnect -= Connected;
    }

    public void Connected()
    {
        _connected = true;
    }

    public void Update()
    {
        if (_connected)
        {
            _content.SetActive(false);
            _connected = false;
        }
    }
}
