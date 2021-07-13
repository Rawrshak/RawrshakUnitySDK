using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Rawrshak;

public class RawrshakSettings : ScriptableObject
{
    public string developerName;
    public string assetBundleFolder;
    public SupportedBuildTargets buildTarget;

    public void Init()
    {
        developerName = "Default Developer";
        assetBundleFolder = "AssetBundles";
        buildTarget = SupportedBuildTargets.StandaloneWindows;
    }
}
