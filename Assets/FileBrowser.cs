using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileBrowser : MonoBehaviour
{
    public GameObject fileListingPrefab;

    private string _selectedFileName;
    private string _currentPath;
    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            _currentPath = value;
            // Update the file listing
            DirectoryInfo directory = new DirectoryInfo(value);
            //FileInfo[] files = directory.GetFiles();
            //foreach (FileInfo file in files)
            //{
            //    GameObject newListing = Instantiate(fileListingPrefab, transform);
            //    FileListing fileListing = newListing.GetComponent<FileListing>();
            //    fileListing.FileName = file.Name;
            //    // Subscribe to event so we know when button is pressed
            //    fileListing.Selected += onFileListingSelected;
            //}
            // We want both files and folders to show
            FileSystemInfo[] fileSystemInfos = directory.GetFileSystemInfos();
            foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
            {
                if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // It's a directory
                    Debug.Log($"{fileSystemInfo.Name} is a directory.");
                }
                else
                {
                    // It's a file
                    Debug.Log($"{fileSystemInfo.Name} is a file.");
                }
                GameObject newListing = Instantiate(fileListingPrefab, transform);
                FileListing fileListing = newListing.GetComponent<FileListing>();
                fileListing.FileName = fileSystemInfo.Name;
                // Subscribe to event so we know when button is pressed
                fileListing.Selected += onFileListingSelected;
            }
        }
    }

    private void onFileListingSelected(object sender, EventArgs e)
    {
        var fileListing = (FileListing)sender;
        _selectedFileName = fileListing.FileName;
    }

    void Start()
    {
        CurrentPath = Path.Combine(Application.persistentDataPath, "TransferDirectory");
    }

    void Update()
    {

    }

    void GoToParentDirectory()
    {
        DirectoryInfo directory = new DirectoryInfo(CurrentPath);
        DirectoryInfo parentDirectory = directory.Parent;
        CurrentPath = parentDirectory.FullName;
    }

    void OpenSelectedFile()
    {
        string fullFilePath = Path.Combine(CurrentPath, _selectedFileName);
        Debug.Log($"Opening file: {fullFilePath}");
    }
}
