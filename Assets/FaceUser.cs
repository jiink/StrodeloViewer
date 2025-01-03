using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceUser : MonoBehaviour
{
    public bool backwards = false;

    // Reference to the main camera
    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        // Get the main camera in the scene
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Make the GameObject face the camera
        if (mainCamera != null)
        {
            Vector3 direction = mainCamera.transform.position - transform.position;
            direction.y = 0; // Keep the object upright
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                if (backwards)
                {
                    transform.Rotate(0, 180, 0);
                }
            }
        }
    }
}
