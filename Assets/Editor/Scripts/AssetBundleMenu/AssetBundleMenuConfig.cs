using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Rawrshak;

namespace Rawrshak 
{
    public class AssetBundleMenuConfig : ScriptableObject
    {
        public string assetBundleFolder;
        public SupportedBuildTargets buildTarget;

        public void Init()
        {
            Debug.Log("Initializing AssetBundleMenuConfig.");
            buildTarget = SupportedBuildTargets.StandaloneWindows;
            assetBundleFolder = "AssetBundles/StandaloneWindows";
        }
    }

}