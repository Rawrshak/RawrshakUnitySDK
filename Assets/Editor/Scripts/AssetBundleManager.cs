using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AssetBundleManager : ScriptableObject
{
    // Private Properties
    AssetBundle assetBundle;
    AssetBundleManifest manifest;

    string assetBundlePath;

    AssetBundlesInfo assetBundlesInfo;

    public void Init(string path)
    {
        assetBundlePath = path;
        LoadAssetBundle();

        if (assetBundlesInfo == null)
        {
            TextAsset jsonFile = Resources.Load("AssetBundleInfo") as TextAsset;
            if (jsonFile)
            {
                assetBundlesInfo = AssetBundlesInfo.CreateFromJSON(jsonFile.text);
            }
            else
            {
                assetBundlesInfo = new AssetBundlesInfo();
            }
        }
    }

    public string[] GetAllAssetBundleNames()
    {
        LoadAssetBundle();
        return manifest.GetAllAssetBundles();
    }

    public Hash128 GetAssetBundleHash(string name)
    {
        LoadAssetBundle();
        return manifest.GetAssetBundleHash(name);
    }

    public void CleanUp()
    {
        Debug.Log("Unloading Asset Bundles.");
        if (assetBundle)
        {
            assetBundle.Unload(true);   
        }
        manifest = null;
    }

    private void LoadAssetBundle()
    {
        if (assetBundle == null)
        {
            Debug.Log("Loading Asset Bundle.");
            assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
        }

        if (manifest == null)
        {
            manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }
}
