using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Rawrshak;

namespace Rawrshak 
{
    public class AssetBundleMenuConfig : ScriptableObject
    {
        static AssetBundleMenuConfig _instance = null;

        public string selectedTargetBuildAssetBundleFolder;
        public SupportedBuildTargets selectedBuildTarget;

        public static AssetBundleMenuConfig Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<AssetBundleMenuConfig>();
                if (!_instance)
                {
                    _instance = ScriptableObject.CreateInstance<AssetBundleMenuConfig>();
                    _instance.selectedBuildTarget = SupportedBuildTargets.StandaloneWindows;
                    _instance.selectedTargetBuildAssetBundleFolder = String.Format("{0}/{1}", "Rawrshak/AssetBundles", _instance.selectedBuildTarget.ToString());
                }
                return _instance;
            }
        }
    }
}