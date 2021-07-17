using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.IO;

namespace Rawrshak
{
    public class AssetBundleManager : ScriptableObject
    {
        // Private Properties
        private string mAssetBundlesInfoLocation;
        private AssetBundle mAssetBundle;
        private AssetBundleManifest mManifest;

        private string mAssetBundlePath;

        private AssetBundlesInfo mAssetBundlesInfo;

        private Box mAssetBundleEntries;
        private Box mUploadedAsssetBundleEntries;
        private Box mAssetBundleInfoBox;
        private Box mHelpBox;

        private RawrshakSettings mRawrshakSettings;
        Dictionary<string, AssetBundleData> mNewAssetBundles;

        public void Init(RawrshakSettings rawrshakSettings)
        {
            mRawrshakSettings = rawrshakSettings;
            mAssetBundlePath = Application.dataPath + "/" + mRawrshakSettings.assetBundleFolder;
            mNewAssetBundles = new Dictionary<string, AssetBundleData>();
            mAssetBundlesInfoLocation = "Assets/Editor/Resources/AssetBundlesInfo.json";
            LoadAssetBundle();

            if (mAssetBundlesInfo == null)
            {
                TextAsset jsonFile = Resources.Load("AssetBundlesInfo") as TextAsset;
                if (jsonFile)
                {
                    mAssetBundlesInfo = AssetBundlesInfo.CreateFromJSON(jsonFile.text);
                    Debug.Log("AssetBundlesInfo Length: " + mAssetBundlesInfo.mDictionary.Count);
                }
                else
                {
                    mAssetBundlesInfo = new AssetBundlesInfo();
                    mAssetBundlesInfo.mDictionary = new Dictionary<Hash128, AssetBundleData>();
                    mAssetBundlesInfo.mData = new List<AssetBundleData>();
                    Debug.Log("AssetBundlesInfo Length: 0 - Creating new asset bundle info.");
                }
            }

            // Load AssetBundleInfo from file
        }

        public string[] GetAllAssetBundleNames()
        {
            LoadAssetBundle();
            return mManifest.GetAllAssetBundles();
        }

        public Hash128 GetAssetBundleHash(string name)
        {
            LoadAssetBundle();
            return mManifest.GetAssetBundleHash(name);
        }

        public void Refresh()
        {
            // clear all entries first
            mAssetBundleEntries.Clear();
            mNewAssetBundles.Clear();

            // Load Entry UML
            var entry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/NewAssetBundleEntry.uxml");

            LoadAssetBundle();
            var bundleNames = mManifest.GetAllAssetBundles();
            foreach(string name in bundleNames)
            {
                var hash = GetAssetBundleHash(name);
                Debug.Log("AssetBundle: " + name + ", hash: " + hash);

                if (mAssetBundlesInfo.mDictionary.ContainsKey(hash))
                {
                    continue;
                }
                
                TemplateContainer entryTree = entry.CloneTree();
                entryTree.contentContainer.Query<Label>("asset-bundle-name").First().text = name;
                entryTree.contentContainer.Query<Label>("asset-bundle-hash").First().text = hash.ToString();
                
                mAssetBundleEntries.Add(entryTree);

                // find or add the asset bundle in the new asset bundle lists
                AssetBundleData assetBundleData = new AssetBundleData(hash, name);
                mNewAssetBundles.Add(name, assetBundleData);

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

            Debug.Log("New Asset Bundle Size: " + mNewAssetBundles.Count);
        }

        public void CleanUp()
        {
            Debug.Log("Unloading Asset Bundles.");
            if (mAssetBundle)
            {
                mAssetBundle.Unload(true);   
            }
            mManifest = null;
            // SaveAssetBundlesInfo();
        }
        
        public void LoadUI(VisualElement root)
        {
            mHelpBox = root.Query<Box>("helpbox-holder").First();

            var generateAssetBundles = root.Query<Button>("create-asset-bundles-button").First();
            generateAssetBundles.clicked += () => {
                CreateAssetBundles.BuildAllAssetBundles(mRawrshakSettings.buildTarget, mRawrshakSettings.assetBundleFolder);
                Refresh();
            };

            mAssetBundleEntries = root.Query<Box>("asset-bundle-entries").First();
            mUploadedAsssetBundleEntries = root.Query<Box>("uploaded-asset-bundle-entries").First();
            mAssetBundleInfoBox = root.Query<Box>("asset-bundle-info").First();

            // var printButton = root.Query<Button>("print-button").First();
            // printButton.clicked += () => {
            //     Refresh();
            // };
            
            var uploadButton = root.Query<Button>("upload-button").First();
            uploadButton.clicked += () => {
                UploadAssetBundles();

                // Update UI
                Refresh();
                RefreshUploadedAssetBundlesBox();
            };

            // Refresh some UI
            Refresh();
            RefreshUploadedAssetBundlesBox();
        }

        public void UploadAssetBundles()
        {
            // Get the selected Asset Bundles
            var iter = mNewAssetBundles.GetEnumerator();
            List<string> bundlesToUpload = new List<string>();
            while(iter.MoveNext())
            {
                if (iter.Current.Value.mSelectedForUploading)
                {
                    Debug.Log(mAssetBundlePath + iter.Current.Value.mName);
                    bundlesToUpload.Add(iter.Current.Key);
                }
            }
            Debug.Log("Dictionary Size left: " + mNewAssetBundles.Count);

            // Upload the asset bundles to storage
            var uploadIter = bundlesToUpload.GetEnumerator();
            while (uploadIter.MoveNext())
            {
                var bundle = mNewAssetBundles[uploadIter.Current];
                if (mAssetBundlesInfo.mDictionary.ContainsKey(bundle.mHashId))
                {
                    // ignore; Don't need to upload what has already been uploaded.
                    continue;
                }

                // Todo: upload to storage

                // Remove from new asset bundles list
                mNewAssetBundles.Remove(uploadIter.Current);

                // Update Asset Bundle Data
                bundle.mSelectedForUploading = false;
                bundle.mUploadedTimestamp = DateTime.Now.ToString();
                
                // Todo: Update Transaction ID and Transaction URI

                // Add to mAssetBundlesInfo
                mAssetBundlesInfo.mDictionary.Add(bundle.mHashId, bundle);
            }
        }

        public void RefreshUploadedAssetBundlesBox()
        {
            // clear all entries first
            mUploadedAsssetBundleEntries.Clear();

            // Load Entry UML
            var entry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/UploadedAssetBundleEntry.uxml");

            // Upload Asset Bundle Box with new entries
            var iter = mAssetBundlesInfo.mDictionary.GetEnumerator();
            while (iter.MoveNext())
            {
                var bundle = iter.Current.Value;
                Debug.Log("Uploaded Asset Bundle: " + bundle.mName);
                
                TemplateContainer entryTree = entry.CloneTree();
                entryTree.contentContainer.Query<Label>("asset-bundle-name").First().text = bundle.mName;
                entryTree.contentContainer.Query<Label>("asset-bundle-uri").First().text = bundle.mHash;
                
                mUploadedAsssetBundleEntries.Add(entryTree);
                
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
            mAssetBundleInfoBox.Clear();
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
            
            mAssetBundleInfoBox.Add(entryTree);
        }

        private void LoadAssetBundle()
        {
            if (mAssetBundle == null)
            {
                Debug.Log("Loading Asset Bundle.");
                // Todo: replace "/AssetBundles" with the folderName
                mAssetBundle = AssetBundle.LoadFromFile(mAssetBundlePath + "/AssetBundles");
            }

            if (mManifest == null)
            {
                mManifest = mAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
        }

        public void SaveAssetBundlesInfo()
        {
            Debug.Log("AssetBundlesInfo length: " + mAssetBundlesInfo.mDictionary.Count);
            if (mAssetBundlesInfo == null || mAssetBundlesInfo.mDictionary == null) {
                return;
            }
            string data = mAssetBundlesInfo.SaveToJSON();
            Debug.Log("Json File: " + data);
            
            // Delete file and .meta filefirst
            File.Delete(mAssetBundlesInfoLocation);
            File.Delete(mAssetBundlesInfoLocation + ".meta");
            
            // write file 
            StreamWriter writer = new StreamWriter(mAssetBundlesInfoLocation, true);
            writer.WriteLine(data);
            writer.Close();
            AssetDatabase.Refresh();
        }
    }

}