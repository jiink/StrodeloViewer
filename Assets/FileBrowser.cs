using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

// The file browser is used for picking a 3d model to import,
// but it's also used by the material inspector to pick an image
// file to use as a texture.
public class FileBrowser : MonoBehaviour
{
    public enum Usage
    {
        ModelImport,
        TexturePicker
    };
    public Usage usage = Usage.ModelImport;

    public GameObject fileListingPrefab;
    public Transform fileListingsParent;
    public TextMeshProUGUI pathLabel;

    private string _selectedFileName;
    private string _currentPath;
    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            _currentPath = value;
            if (string.IsNullOrEmpty(_currentPath))
            {
                pathLabel.text = "NULL";
                return;
            }
            // Update the file listing
            // Clear out the old listings
            foreach (Transform child in fileListingsParent)
            {
                Destroy(child.gameObject);
            }
            DirectoryInfo directory = new DirectoryInfo(value);
            // We want both files and folders to show
            FileSystemInfo[] fileSystemInfos = directory.GetFileSystemInfos();
            foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
            {
                GameObject newListing = Instantiate(fileListingPrefab, fileListingsParent);
                FileListing fileListing = newListing.GetComponent<FileListing>();
                fileListing.FileName = fileSystemInfo.Name;
                if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // It's a directory
                    Debug.Log($"{fileSystemInfo.Name} is a directory.");
                    fileListing.Selected += (sender, e) =>
                    {
                        CurrentPath = Path.Combine(CurrentPath, fileListing.FileName);
                    };
                }
                else
                {
                    // It's a file
                    Debug.Log($"{fileSystemInfo.Name} is a file.");
                    // Subscribe to event so we know when button is pressed
                    fileListing.Selected += onFileListingSelected;
                }
                
                
            }
            pathLabel.text = value;
        }
    }
    public string FullFilePath
    {
        get
        {
            return Path.Combine(CurrentPath, _selectedFileName);
        }
    }

    public event EventHandler FileOpen;

    private void onFileListingSelected(object sender, EventArgs e)
    {
        var fileListing = (FileListing)sender;
        _selectedFileName = fileListing.FileName;
        // Highlight this button and unhighlight the others
        foreach (Transform child in fileListingsParent)
        {
            FileListing fl = child.GetComponent<FileListing>();
            if (fl == fileListing)
            {
                fl.IsHighlighted = true;
            }
            else
            {
                fl.IsHighlighted = false;
            }
        }
    }

    void Start()
    {
        CurrentPath = Path.Combine(Application.persistentDataPath, "TransferDirectory");
    }

    void Update()
    {

    }

    public void GoToParentDirectory()
    {
        DirectoryInfo directory = new DirectoryInfo(CurrentPath);
        DirectoryInfo parentDirectory = directory.Parent;
        CurrentPath = parentDirectory.FullName;
    }

    public void OpenSelectedFile()
    {
        string fullFilePath = Path.Combine(CurrentPath, _selectedFileName);
        Debug.Log($"Opening file: {fullFilePath}");
        // let listener know what file was selected.
        // the listener can read FullFilePath to get the file that should be opened.
        FileOpen?.Invoke(this, EventArgs.Empty);
    }
}
