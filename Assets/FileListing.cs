using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FileListing : MonoBehaviour
{
    public Sprite fileIcon;
    public Sprite folderIcon;
    public Image iconComponent;
    public Image buttonBackground;
    public TextMeshProUGUI text;
    public Material highlightMaterial;
    private Material normalMaterial;
    private string _fileName;
    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            text.text = value;
        }
    }
    private bool _isHighlighted = false;
    public bool IsHighlighted
    {
        get => _isHighlighted;
        set
        {
            _isHighlighted = value;
            if (_isHighlighted)
            {
                buttonBackground.material = highlightMaterial;
            }
            else
            {
                buttonBackground.material = normalMaterial;
            }
        }
    }
    public event EventHandler Selected;

    void Start()
    {
        normalMaterial = buttonBackground.material;
    }

    void Update()
    {
        
    }

    public void Select()
    {
        Selected?.Invoke(this, EventArgs.Empty);
    }
}
