using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FileListing : MonoBehaviour
{
    public Image buttonBackground;
    public TextMeshProUGUI text;
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
                buttonBackground.color = _highlightColor;
            }
            else
            {
                buttonBackground.color = _normalColor;
            }
        }
    }
    public event EventHandler Selected;

    private Color _highlightColor;
    private Color _normalColor;

    void Start()
    {
        _highlightColor = Color.cyan;
        _normalColor = buttonBackground.color;
    }

    void Update()
    {
        
    }

    public void Select()
    {
        Selected?.Invoke(this, EventArgs.Empty);
    }
}
