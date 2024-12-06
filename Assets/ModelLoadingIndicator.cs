using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ModelLoadingIndicator : MonoBehaviour
{
    public TextMeshPro text;
    public string ModelName
    {
        set
        {
            text.text = "Loading " + value + "...";
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (text == null)
        {
            Debug.LogError("TextMeshProUGUI component not set in ModelLoadingIndicator");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
