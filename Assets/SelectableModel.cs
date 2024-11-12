using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableModel : MonoBehaviour
{
    public event EventHandler Selected;

    private Quaternion lockedRotation; // write down rotation once lock happens
    private bool isRotationLocked = false;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (isRotationLocked)
        {
            gameObject.transform.rotation = lockedRotation;
        }
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

    internal void LockRotation(bool rotationLock)
    {
        // Freeze rotation on rigidbody component if there is one
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = rotationLock;
        }
        else
        {
            Debug.LogError("SelectableModel: No rigidbody found");
        }
        if (rotationLock)
        {
            // write down the current rotation so we can keep it there
            lockedRotation = gameObject.transform.rotation;
        }
        isRotationLocked = rotationLock;
    }
}
