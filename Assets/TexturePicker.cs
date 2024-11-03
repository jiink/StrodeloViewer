using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePicker : MonoBehaviour
{
    private Texture2D _texture;
    private GameObject _fileBrowserPrefab;
    void Start()
    {
        _fileBrowserPrefab = Resources.Load<GameObject>("FileBrowser Variant");
    }

    void Update()
    {
        
    }

    public void OpenFileBrowser()
    {
        Vector3 spawnPos = transform.position + transform.forward * -0.1f;
        Quaternion rot = transform.rotation * Quaternion.Euler(0, 180, 0);
        var fileBrowser = Instantiate(_fileBrowserPrefab, spawnPos, rot);
    }
}
