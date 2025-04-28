using UnityEngine;
using System.Text.RegularExpressions;
public class GoogleDriveHelper
{
    public static string ConvertToDirectDownloadLink(string rawLink)
    {
        //https://drive.google.com/drive/folders/1cwyCXJz4ucu-nA6M7dVlbUsd6BRE6izq?usp=drive_link
        Match match = Regex.Match(rawLink, @"(?:drive\.google\.com\/.*\/d\/|\/d\/)([a-zA-Z0-9_-]+)");

        string result = string.Empty;

        if (match.Success) 
        {
            string id = match.Groups[1].Value;
            result = "https://drive.google.com/uc?export=download&id=" + id;
            Debug.Log(result);
        }
        else
        {
            Debug.LogError("invalid google drive link");
        }

        return result;
    }
}
