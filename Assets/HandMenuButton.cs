using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandMenuButton : MonoBehaviour
{
    public GameObject label;

    // Start is called before the first frame update
    void Start()
    {
        label.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnHover(BaseEventData eventData)
    {
        Debug.Log("Hovering");
        label.SetActive(true);
    }

    public void OnHoverEnd(BaseEventData eventData) {
        Debug.Log("Hover End");
        label.SetActive(false);
    }
}
