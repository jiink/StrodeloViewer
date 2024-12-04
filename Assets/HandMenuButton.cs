using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandMenuButton : MonoBehaviour
{
    public GameObject icon;
    public GameObject label;

    private Action clickAction;

    void Start()
    {
        label.SetActive(false);
    }

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

    public void SetData(HButtonEntry hButtonEntry)
    {
        label.GetComponent<TextMeshProUGUI>().text = hButtonEntry.Name;
        icon.GetComponent<UnityEngine.UI.Image>().sprite = hButtonEntry.Icon;
        clickAction = hButtonEntry.OnClick;
    }
}
