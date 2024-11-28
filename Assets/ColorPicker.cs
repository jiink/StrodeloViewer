// Used Info Gamer's "How to Make a Color Picker in Unity" tutorial (https://www.youtube.com/watch?v=M6kEW6feh7g)
// Currently this just does not work. Using hue slider and saturation slider instead.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
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

        cursor.position = pointer.position; // in the light editing menu the x is like 920 to 990 and y is like 1070 to 1140
        RectTransform colorChartRect = chart.GetComponent<RectTransform>();
        Vector2 cursorRealPos = new Vector2( // Number gets RIDICULOUSLY high idk why
            colorChartRect.rect.width/2 + cursor.localPosition.x,
            colorChartRect.rect.height / 2 + cursor.localPosition.y
            );
        Vector2 pix = new Vector2( // unreasonable number because of cursorRealPos
            cursorRealPos.x * (colorChart.width / colorChartRect.rect.width),
            cursorRealPos.y * (colorChart.height / colorChartRect.rect.height)
            );
        Debug.Log($"pointer.p: {pointer.position}");
        Debug.Log($"cursorRealPos: {cursorRealPos}");
        Debug.Log($"pix: {pix}"); 
        Color pickedColor = colorChart.GetPixel(
            (int)(pix.x),
            (int)(pix.y)
            );
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
