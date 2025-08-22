using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "FileMetaData", menuName = "Metadata/File Metadata")]
public class MetadataScriptableObject : ScriptableObject
{
    [Header("Meta Data")]
    public string filename = "file_metadata";
    public string extension = ".json";
    public string metadataFileLink;

    [Header("File Data")]
    public string associatedFileLink;
    public string associatedFileExtension;
    public string version;


    public string LocalMetadataFilePath => Path.Combine(Application.streamingAssetsPath, filename + extension);

    public string LocalFilePath => Path.Combine(Application.streamingAssetsPath, filename + associatedFileExtension);

    public string DirectMetadataDownloadLink => GoogleDriveHelper.ConvertToDirectDownloadLink(metadataFileLink);

    public string RemoteFileDownloadLink { get; set; }

    public void SetupLocalMetaData()
    {
        version = string.Empty;

        if (File.Exists(LocalMetadataFilePath))
        {
            // if we are here there's a local meta data, yay
            string localMetaDataContent = File.ReadAllText(LocalMetadataFilePath).ToString();
            MetaDataFile localMetaData = JsonUtility.FromJson<MetaDataFile>(localMetaDataContent);

            if (localMetaData != null)
            {
                // here I want to just grab the current version.
                version = localMetaData.version;
            }
            File.Delete(LocalMetadataFilePath);
        }
        else
        {
            // effectively there's no previous meta data, just download a fresh one.
            version = "-1";
        }
    }
    public bool FileNeedsUpdating()
    {
        // log the file path for confirmation
        Debug.Log("Checking local metadata file at path:" + LocalMetadataFilePath);

        // step 1 check if the metadata file exists.
        if (!File.Exists(LocalMetadataFilePath))
        {
            Debug.Log("Metadata file does not exist, update is required");
            return true;
        }

        // step 2 Read and log the metadata content
        string metadataContent = File.ReadAllText(LocalMetadataFilePath);

        if (string.IsNullOrEmpty(metadataContent))
        {
            Debug.Log("Metadata file is empty, update is required");
            return true;
        }

        // step 3 parse the json
        MetaDataFile remoteMetaData = JsonUtility.FromJson<MetaDataFile>(metadataContent);

        if (remoteMetaData == null)
        {
            Debug.LogError("Failed to parse metadata content, update required");
            return true;
        }

        // step 4 compare the versions
        if (version != remoteMetaData.version)
        {
            Debug.Log($"New version detected: {remoteMetaData.version}. updating from {version}");
            version = remoteMetaData.version; // update our local version

            RemoteFileDownloadLink = GoogleDriveHelper.ConvertToDirectDownloadLink(remoteMetaData.filelink);
            return true;
        }

        Debug.Log($"{filename} is up-to-date");
        return false;
    }

    public void DeleteLocalFile()
    {
        if (File.Exists(LocalFilePath))
        {
            File.Delete(LocalFilePath);
            Debug.Log($"Deleting outdated file at {LocalFilePath}");
        }
    }
}
