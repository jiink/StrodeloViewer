using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrodeloLight : MonoBehaviour
{
    public event EventHandler OnSelectAction;
    private RayInteractable interactable;
    // Start is called before the first frame update
    void Start()
    {
        if (!gameObject.TryGetComponent<RayInteractable>(out interactable))
        {
            Debug.LogError("No RayInteractable found on StrodeloLight");
        }
        //interactable = GetComponent<RayInteractable>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        OnSelectAction?.Invoke(this, EventArgs.Empty);
    }

    public void EnableCollider()
    {
        if (interactable == null)
        {
            if (!gameObject.TryGetComponent<RayInteractable>(out interactable))
            {
                Debug.LogError("No RayInteractable found on StrodeloLight");
            }
        }
        interactable.enabled = true;
    }

    // So pointing at the light doesn't interfere
    // with other interactions that might be going on
    public void DisableCollider()
    {
        if (interactable == null)
        {
            if (!gameObject.TryGetComponent<RayInteractable>(out interactable))
            {
                Debug.LogError("No RayInteractable found on StrodeloLight");
            }
        }
        interactable.enabled = false;
    }
}
