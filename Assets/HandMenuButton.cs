using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandMenuButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI label;
    [SerializeField]
    private Image imageIconElement;
    private string _labelText;
    public string LabelText
    {
        get { return _labelText; }
        set
        {
            _labelText = value;
            label.text = _labelText;
        }
    }
    private Sprite _icon;
    public Sprite Icon
    {
        get { return _icon; }
        set
        {
            if (value == null)
            {
                Debug.LogWarning("Icon is null");
                return;
            }
            _icon = value;
            imageIconElement.sprite = _icon;
        }
    }
    private Action clickAction;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void SetData(HButtonEntry hButtonEntry)
    {
        LabelText = hButtonEntry.Name;
        Icon = hButtonEntry.Icon;
        clickAction = hButtonEntry.OnClick;
    }

    public void OnClick()
    {
        clickAction();
    }
}
