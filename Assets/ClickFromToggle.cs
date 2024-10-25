using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Practically turns a toggle button into a normal press button.
// Pretty much just removes the true/false value from the event.
public class ClickFromToggle : MonoBehaviour
{
    public UnityEvent onClick;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleCallback(bool value)
    {
        onClick.Invoke();
    }
}
