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

        public string assetBundleFolder;
        public SupportedBuildTargets buildTarget;

        public static AssetBundleMenuConfig Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<AssetBundleMenuConfig>();
                if (!_instance)
                {
                    _instance = ScriptableObject.CreateInstance<AssetBundleMenuConfig>();
                    _instance.buildTarget = SupportedBuildTargets.StandaloneWindows;
                    _instance.assetBundleFolder = String.Format("{0}/{1}", "Rawrshak/AssetBundles", _instance.buildTarget.ToString());
                }
                return _instance;
            }
        }
    }
}