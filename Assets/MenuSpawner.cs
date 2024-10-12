using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns a menu in front of the user.
public class MenuSpawner : MonoBehaviour
{
    public FloatingMenu menuPrefab;
    public Transform userTransform;
    public float spawnDistance = 0.5f; // How far in front of the user to spawn the menu.

    // Start is called before the first frame update
    void Start()
    {
        if (menuPrefab == null && userTransform == null)
        {
            Debug.LogError("MenuSpawner: Menu Prefab or User Transform not set.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnMenu()
    {
        Vector3 spawnPos = userTransform.position + userTransform.forward * spawnDistance;
        // face the menu towards the user
        Quaternion rotation = Quaternion.LookRotation(userTransform.position - spawnPos);
        FloatingMenu menu = Instantiate(menuPrefab, spawnPos, rotation);
    }
}
