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
        Debug.Log($">>>> CLICKED ON {gameObject.name}!");
        Selected?.Invoke(gameObject, EventArgs.Empty);
    }

    public void ShowVisual()
    {
        if (GetVisualizerChild() != null)
        {
            GetVisualizerChild().SetActive(true);
        }
    }

    public void HideVisual()
    {
        if (GetVisualizerChild() != null)
        {
            GetVisualizerChild().SetActive(false);
        }
    }

    public GameObject GetVisualizerChild()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("SelectionVisualizer"))
            {
                return child.gameObject;
            }
        }
        return null;
    }
}
