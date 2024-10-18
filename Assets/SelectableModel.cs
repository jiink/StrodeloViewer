using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableModel : MonoBehaviour
{
    public event EventHandler Selected;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Select()
    {
        Debug.Log(">>>> I AM SELECTED!");
        Selected?.Invoke(gameObject, EventArgs.Empty);
    }
}
