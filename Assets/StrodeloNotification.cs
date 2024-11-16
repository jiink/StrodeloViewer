using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrodeloNotification : MonoBehaviour
{
    private string _message;
    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            GetComponentInChildren<TMPro.TextMeshProUGUI>().text = _message;
        }
    }

    void Start()
    {
        Destroy(gameObject, 4f);
    }

    void Update()
    {
        
    }
}
