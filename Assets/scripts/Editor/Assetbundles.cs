using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.IO;

public class Assetbundles
{
    [MenuItem("assets/buidler assetbundle")]

    static void buildallassetsbundles()
    {
        string assetBundleDirectory = Path.Combine(Application.streamingAssetsPath, "AssetBundles");

        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory,BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
}