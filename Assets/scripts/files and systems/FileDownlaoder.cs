using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FileDownlaoder : MonoBehaviour
{
    //https://drive.google.com/drive/folders/1cwyCXJz4ucu-nA6M7dVlbUsd6BRE6izq?usp=drive_link

    [SerializeField] private List<MetadataScriptableObject> filesToDownload = new List<MetadataScriptableObject>();
    void Start()
    {
        //Debug.Log(GoogleDriveHelper.ConvertToDirectDownloadLink(downloadPath));
        StartCoroutine(CheckAndDownloadFiles());
    }
    private IEnumerator CheckAndDownloadFiles()
    {
        // just download each file, don't check if they are up to date.
        foreach (MetadataScriptableObject metaData in filesToDownload)
        {
            metaData.SetupLocalMetaData();
            yield return StartCoroutine(DownloadFile(metaData.DirectMetadataDownloadLink, metaData.LocalMetadataFilePath));
            yield return new WaitForEndOfFrame();
            if (metaData.FileNeedsUpdating() || !File.Exists(metaData.LocalFilePath))
            {
                metaData.DeleteLocalFile();
                if (!string.IsNullOrEmpty(metaData.LocalMetadataFilePath) && !string.IsNullOrEmpty(metaData.DirectMetadataDownloadLink))
                {
                    yield return StartCoroutine(DownloadFile(metaData.RemoteFileDownloadLink, metaData.LocalFilePath));
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    Debug.LogError("Failed to obtain a valid download link, or local meta data path is not valid.");
                }
            }
            else
            {
                Debug.Log($"{metaData.filename} is up-to-date");
            }
            yield return null;
        }
        yield return null;
    }
    private IEnumerator DownloadFile(string fileLink, string savePath)
    {
        if (string.IsNullOrEmpty(fileLink))
        {
            Debug.LogError("Invalid file Link");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(fileLink); // creates the request for download

        yield return request.SendWebRequest(); // send it off and start downloading

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to download file: {request.error}");
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)); // check directory exists
            File.WriteAllBytes(savePath, request.downloadHandler.data); // write the files to the destination.
            Debug.Log($"File Downloaded Successfully to: {savePath}");
        }
    }
}
