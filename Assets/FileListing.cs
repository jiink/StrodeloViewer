using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FileListing : MonoBehaviour
{
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
    public event EventHandler Selected;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Select()
    {
        Selected?.Invoke(this, EventArgs.Empty);
    }
}
