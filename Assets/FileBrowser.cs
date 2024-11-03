using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileBrowser : MonoBehaviour
{
    public GameObject fileListingPrefab;

    private string _currentPath;
    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            _currentPath = value;
            // Update the file listing
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
