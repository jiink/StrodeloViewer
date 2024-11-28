using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEditMenu : MonoBehaviour
{
    private Light _inspectedLight;
    public Light InspectedLight
    {
        get { return _inspectedLight; }
        set
        {
            _inspectedLight = value;
        }
    }

    void Start()
    {

    }


    void Update()
    {

    }

    public void SetLightColor(Color c)
    {
        if (InspectedLight != null)
        {
            InspectedLight.color = c;
        }
        else
        {
            Debug.LogError("No light assigned to LightEditMenu.");
        }
    }

    public void SetLightHue(float h)
    {
        if (InspectedLight != null)
        {
            Color.RGBToHSV(InspectedLight.color, out _, out float s, out float v);
            InspectedLight.color = Color.HSVToRGB(h, s, v);
        }
        else
        {
            Debug.LogError("No light assigned to LightEditMenu.");
        }
    }

    public void SetLightSaturation(float s)
    {
        if (InspectedLight != null)
        {
            Color.RGBToHSV(InspectedLight.color, out float h, out _, out float v);
            InspectedLight.color = Color.HSVToRGB(h, s, v);
        }
        else
        {
            Debug.LogError("No light assigned to LightEditMenu.");
        }
    }

    public void SetLightIntensity(float i)
    {
        if (InspectedLight != null)
        {
            InspectedLight.intensity = i;
        }
        else
        {
            Debug.LogError("No light assigned to LightEditMenu.");
        }
    }

    public void SetShadows(bool s)
    {
        if (InspectedLight != null)
        {
            InspectedLight.shadows = s ? LightShadows.Hard : LightShadows.None;
        }
        else
        {
            Debug.LogError("No light assigned to LightEditMenu.");
        }
    }
}