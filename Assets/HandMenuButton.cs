using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandMenuButton : MonoBehaviour
{
    public Image icon;
    public GameObject tooltip;
    public TextMeshProUGUI label;
    public Sprite hoverBg;
    private Sprite normalBg;
    private Image bgImageComponent;

    private Action clickAction;

    void Start()
    {
        bgImageComponent = GetComponent<UnityEngine.UI.Image>();
        if (bgImageComponent == null)
        {
            Debug.LogError("HandMenuButton must have an Image component");
        }
        normalBg = bgImageComponent.sprite;
    }

    public void OnHover(BaseEventData eventData)
    {
        bgImageComponent.sprite = hoverBg;
        tooltip.SetActive(true);
    }

    public void OnHoverEnd(BaseEventData eventData)
    {
        bgImageComponent.sprite = normalBg;
        tooltip.SetActive(false);
    }

    public void SetData(HButtonEntry hButtonEntry)
    {
        icon.GetComponent<Image>().sprite = hButtonEntry.Icon;
        label.text = hButtonEntry.Name;
        clickAction = hButtonEntry.OnClick;
    }

    public void OnClick()
    {
        clickAction();
    }
}
