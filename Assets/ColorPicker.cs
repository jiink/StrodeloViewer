// Used Info Gamer's "How to Make a Color Picker in Unity" tutorial (https://www.youtube.com/watch?v=M6kEW6feh7g)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public UnityEvent<Color> ColorPickerEvent;
    [SerializeField] Texture2D colorChart;
    [SerializeField] GameObject chart;
    [SerializeField] RectTransform cursor;
    [SerializeField] Image button;
    [SerializeField] Image cursorColor;

    public void PickColor(BaseEventData data)
    {
        Debug.Log("DING!");
        PointerEventData pointer = data as PointerEventData;
        cursor.position = pointer.position;
        RectTransform colorChartRect = chart.GetComponent<RectTransform>();
        Vector2 cursorRealPos = new Vector2(colorChartRect.rect.width/2 + cursor.localPosition.x, colorChartRect.rect.height / 2 + cursor.localPosition.y);
        Color pickedColor = colorChart.GetPixel(
            (int)(cursorRealPos.x * (colorChart.width / colorChartRect.rect.width)),
            (int)(cursorRealPos.y * (colorChart.height / colorChartRect.rect.height))
            );
        //Color pickedColor = colorChart.GetPixel(
        //    (int)(cursor.localPosition.x * (colorChart.width / transform.GetChild(0).GetComponent<RectTransform>().rect.width)),
        //    (int)(cursor.localPosition.y * (colorChart.height / transform.GetChild(0).GetComponent<RectTransform>().rect.height))
        //    );
        button.color = pickedColor;
        cursorColor.color = pickedColor;
        ColorPickerEvent.Invoke(pickedColor);
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
