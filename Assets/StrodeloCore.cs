using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrodeloCore : MonoBehaviour
{
    public GameObject receiverPrefab;

    void Start()
    {
        var receiver = Instantiate(receiverPrefab);
    }

    void Update()
    {
        
    }
}
