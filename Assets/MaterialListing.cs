using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MaterialListing : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;
    private Texture2D _texture;
    private GameObject _fileBrowserPrefab; // for picking texture
    private Material _material;
    public Material Material
    {
        get
        {
            return _material;
        }
        set
        {
            _material = value;
            UpdateLabel();
        }
    }
    private int _materialNumber;
    public int MaterialNumber
    {
        get
        {
            return _materialNumber;
        }
        set
        {
            _materialNumber = value;
            UpdateLabel();
        }
    }

    void UpdateLabel()
    {
        if (Material == null)
        {
            nameLabel.text = $"Material #{MaterialNumber}: (none)";
            return;
        }
        nameLabel.text = $"Material #{MaterialNumber}: {Material.name}";
    }

    public void SetMetallic(float value)
    {
        Material.SetFloat("_Metallic", value);
    }

    public void SetSmoothness(float value)
    {
        Material.SetFloat("_Glossiness", value);
    }

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
        var fileBrowserObj = Instantiate(_fileBrowserPrefab, spawnPos, rot);
        FileBrowser fileBrowser = fileBrowserObj.GetComponent<FileBrowser>();
        fileBrowser.usage = FileBrowser.Usage.TexturePicker;
        fileBrowser.FileOpen += (sender, e) =>
        {
            SetTextureFromFilePath(fileBrowser.FullFilePath);
        };
    }

    private void SetTextureFromFilePath(string fullFilePath)
    {
        if (string.IsNullOrEmpty(fullFilePath) || !System.IO.File.Exists(fullFilePath))
        {
            Debug.LogError("Invalid file path.");
            return;
        }
        try
        {
            // Dispose of the old texture if it exists
            if (_texture != null)
            {
                Destroy(_texture);
            }

            // Load the texture
            _texture = new Texture2D(2, 2);
            byte[] fileData = System.IO.File.ReadAllBytes(fullFilePath);
            if (!_texture.LoadImage(fileData))
            {
                Debug.LogError("Failed to load texture from file.");
                return;
            }

            Material.SetTexture("_MainTex", _texture);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading texture: {ex.Message}");
        }
    }
}
