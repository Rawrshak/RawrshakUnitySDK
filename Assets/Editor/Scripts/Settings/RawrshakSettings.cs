using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Rawrshak;

namespace Rawrshak 
{
    public class RawrshakSettings : ScriptableObject
    {
        public string developerName;
        public string assetBundleFolder;
        public string defaultKeystoreLocation;
        public SupportedBuildTargets buildTarget;

        public void Init()
        {
            Debug.Log("Initializing RawrshakSettings.");
            developerName = "Default Developer";
            assetBundleFolder = "AssetBundles";
            defaultKeystoreLocation = "Assets/Editor/Resources/Keystore/WalletKeyStore.json";
            buildTarget = SupportedBuildTargets.StandaloneWindows;
        }
    }

}