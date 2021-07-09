using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.IO;

public class AssetBundleManager : ScriptableObject
{
    // Private Properties
    public string assetBundlesInfoLocation;
    AssetBundle assetBundle;
    AssetBundleManifest manifest;

    string assetBundlePath;

    AssetBundlesInfo assetBundlesInfo;

    public Box assetBundleEntries;
    public Box uploadedAsssetBundleEntries;
    public Box assetBundleInfoBox;

    Dictionary<string, AssetBundleData> newAssetBundles;

    public void Init(string path)
    {
        assetBundlePath = path;
        newAssetBundles = new Dictionary<string, AssetBundleData>();
        assetBundlesInfoLocation = "Assets/Editor/Resources/AssetBundlesInfo.json";
        LoadAssetBundle();

        if (assetBundlesInfo == null)
        {
            TextAsset jsonFile = Resources.Load("AssetBundlesInfo") as TextAsset;
            if (jsonFile)
            {
                assetBundlesInfo = AssetBundlesInfo.CreateFromJSON(jsonFile.text);
                Debug.Log("AssetBundlesInfo Length: " + assetBundlesInfo.mDictionary.Count);
            }
            else
            {
                assetBundlesInfo = new AssetBundlesInfo();
                assetBundlesInfo.mDictionary = new Dictionary<Hash128, AssetBundleData>();
                assetBundlesInfo.mData = new List<AssetBundleData>();
                Debug.Log("AssetBundlesInfo Length: 0 - Creating new asset bundle info.");
            }
        }

        // Load AssetBundleInfo from file
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

    public void Refresh()
    {
        // clear all entries first
        assetBundleEntries.Clear();
        newAssetBundles.Clear();

        // Load Entry UML
        var entry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/NewAssetBundleEntry.uxml");

        LoadAssetBundle();
        var bundleNames = manifest.GetAllAssetBundles();
        foreach(string name in bundleNames)
        {
            var hash = GetAssetBundleHash(name);
            Debug.Log("AssetBundle: " + name + ", hash: " + hash);

            if (assetBundlesInfo.mDictionary.ContainsKey(hash))
            {
                continue;
            }
            
            TemplateContainer entryTree = entry.CloneTree();
            entryTree.contentContainer.Query<Label>("asset-bundle-name").First().text = name;
            entryTree.contentContainer.Query<Label>("asset-bundle-hash").First().text = hash.ToString();
            
            assetBundleEntries.Add(entryTree);

            // find or add the asset bundle in the new asset bundle lists
            AssetBundleData assetBundleData = new AssetBundleData(hash, name);
            newAssetBundles.Add(name, assetBundleData);

            // Set Toggle Callback
            var selectedToggle = entryTree.contentContainer.Query<Toggle>("asset-bundle-selected").First();
            selectedToggle.RegisterCallback<ChangeEvent<bool>>((evt) => {
                assetBundleData.mSelectedForUploading = (evt.target as Toggle).value;
                Debug.Log("Selected: " + assetBundleData.mName + " is " + (evt.target as Toggle).value);
                Debug.Log("Current Time: " + DateTime.Now);
            });

            // Select Asset Bundle Callback
            entryTree.RegisterCallback<MouseDownEvent>((evt) => {
                // assetBundleData.selected = (evt.target as Label).value;
                Debug.Log("Info to Display: " + assetBundleData.mName);
                ViewAssetBundleInfo(assetBundleData);
            });
        }

        Debug.Log("New Asset Bundle Size: " + newAssetBundles.Count);
    }

    public void CleanUp()
    {
        Debug.Log("Unloading Asset Bundles.");
        if (assetBundle)
        {
            assetBundle.Unload(true);   
        }
        manifest = null;
        // SaveAssetBundlesInfo();
    }

    public void UploadAssetBundles()
    {
        // Get the selected Asset Bundles
        var iter = newAssetBundles.GetEnumerator();
        List<string> bundlesToUpload = new List<string>();
        while(iter.MoveNext())
        {
            if (iter.Current.Value.mSelectedForUploading)
            {
                Debug.Log(assetBundlePath + iter.Current.Value.mName);
                bundlesToUpload.Add(iter.Current.Key);
            }
        }
        Debug.Log("Dictionary Size left: " + newAssetBundles.Count);

        // Upload the asset bundles to storage
        var uploadIter = bundlesToUpload.GetEnumerator();
        while (uploadIter.MoveNext())
        {
            var bundle = newAssetBundles[uploadIter.Current];
            if (assetBundlesInfo.mDictionary.ContainsKey(bundle.mHashId))
            {
                // ignore; Don't need to upload what has already been uploaded.
                continue;
            }

            // Todo: upload to storage

            // Remove from new asset bundles list
            newAssetBundles.Remove(uploadIter.Current);

            // Update Asset Bundle Data
            bundle.mSelectedForUploading = false;
            bundle.mUploadedTimestamp = DateTime.Now.ToString();
            
            // Todo: Update Transaction ID and Transaction URI

            // Add to assetBundlesInfo
            assetBundlesInfo.mDictionary.Add(bundle.mHashId, bundle);
        }
    }

    public void RefreshUploadedAssetBundlesBox()
    {
        // clear all entries first
        uploadedAsssetBundleEntries.Clear();

        // Load Entry UML
        var entry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/UploadedAssetBundleEntry.uxml");

        // Upload Asset Bundle Box with new entries
        var iter = assetBundlesInfo.mDictionary.GetEnumerator();
        while (iter.MoveNext())
        {
            var bundle = iter.Current.Value;
            Debug.Log("Uploaded Asset Bundle: " + bundle.mName);
            
            TemplateContainer entryTree = entry.CloneTree();
            entryTree.contentContainer.Query<Label>("asset-bundle-name").First().text = bundle.mName;
            entryTree.contentContainer.Query<Label>("asset-bundle-uri").First().text = bundle.mHash;
            
            uploadedAsssetBundleEntries.Add(entryTree);
            
            // Select Asset Bundle Callback to show info
            entryTree.RegisterCallback<MouseDownEvent>((evt) => {
                // assetBundleData.selected = (evt.target as Label).value;
                Debug.Log("Info to Display: " + bundle.mName);
                // Todo: Show Info
                ViewAssetBundleInfo(bundle);
            });
        }

        SaveAssetBundlesInfo();
    }

    private void ViewAssetBundleInfo(AssetBundleData bundle)
    {
        assetBundleInfoBox.Clear();
        var entry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetBundleDetails.uxml");
        TemplateContainer entryTree = entry.CloneTree();
        entryTree.contentContainer.Query<TextField>("name").First().value = bundle.mName;
        entryTree.contentContainer.Query<TextField>("name").First().SetEnabled(false);
        entryTree.contentContainer.Query<TextField>("hash").First().value = bundle.mHash;
        entryTree.contentContainer.Query<TextField>("hash").First().SetEnabled(false);
        entryTree.contentContainer.Query<TextField>("transaction-id").First().value = bundle.mTransactionId;
        entryTree.contentContainer.Query<TextField>("transaction-id").First().SetEnabled(false);
        entryTree.contentContainer.Query<TextField>("uri").First().value = bundle.mUri;
        entryTree.contentContainer.Query<TextField>("uri").First().SetEnabled(false);
        entryTree.contentContainer.Query<TextField>("uploaded-timestamp").First().value = bundle.mUploadedTimestamp;
        entryTree.contentContainer.Query<TextField>("uploaded-timestamp").First().SetEnabled(false);
        
        assetBundleInfoBox.Add(entryTree);
    }

    private void LoadAssetBundle()
    {
        if (assetBundle == null)
        {
            Debug.Log("Loading Asset Bundle.");
            assetBundle = AssetBundle.LoadFromFile(assetBundlePath + "AssetBundles");
        }

        if (manifest == null)
        {
            manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }

    public void SaveAssetBundlesInfo()
    {
        Debug.Log("AssetBundlesInfo length: " + assetBundlesInfo.mDictionary.Count);
        if (assetBundlesInfo == null || assetBundlesInfo.mDictionary == null) {
            return;
        }
        string data = assetBundlesInfo.SaveToJSON();
        Debug.Log("Json File: " + data);
        
        // Delete file and .meta filefirst
        File.Delete(assetBundlesInfoLocation);
        File.Delete(assetBundlesInfoLocation + ".meta");
        
        // write file 
        StreamWriter writer = new StreamWriter(assetBundlesInfoLocation, true);
        writer.WriteLine(data);
        writer.Close();
        AssetDatabase.Refresh();
    }
}
