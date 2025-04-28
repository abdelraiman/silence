using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MetadataScriptableObject))]
public class MetaDataScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MetadataScriptableObject metadata = (MetadataScriptableObject)target;

        if (GUILayout.Button("Export to Json"))
        {
            // I want to export my json please :)
            ExportToJson(metadata);
            EditorUtility.SetDirty(metadata); // something's change we should update and save changes
        }
    }

    private void ExportToJson(MetadataScriptableObject metadata)
    {
        MetaDataFile metadataFile = new MetaDataFile
        {
            version = metadata.version,
            filelink = metadata.associatedFileLink
        };
        string json = JsonUtility.ToJson(metadataFile, true);

        Directory.CreateDirectory(Application.streamingAssetsPath);

        File.WriteAllText(metadata.LocalMetadataFilePath, json);
        Debug.Log($"Metadata Exported to json at: {metadata.LocalMetadataFilePath}");
    }
}
