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
}