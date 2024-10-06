using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenu : MonoBehaviour
{
    public GameObject LeftHandAnchor;
    public Vector3 offset = new Vector3(0, 0.1f, 0);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move object to somewhere just above the left hand
        transform.position = LeftHandAnchor.transform.position + offset;
    }
}
