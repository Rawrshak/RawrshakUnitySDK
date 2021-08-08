using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.IO;

namespace Rawrshak
{
    public class ABManager : ScriptableObject
    {
        // Static Properties
        static string UPLOADED_LIST_FILE = "Assets/Editor/Resources/AssetBundlesMenuConfig/UploadedListAssetBundlesInfo.json";

        // Private Properties
        private string mAssetBundleDirectory; // Asset Bundle Directory
        private AssetBundle mFolderObj; // Asset Bundle Folder Object
        private AssetBundleManifest mManifest; // Asset Bundle Folder Manifest Object
        
        Dictionary<string, ABData> mUntrackedAssetBundles;
        Dictionary<Hash128, ABData> mUploadedAssetBundles;
        
        // UI
        Box mUntrackedAssetBundleHolder;
        Box mHelpBoxHolder;

        public void Init(string directory, string folderObjName)
        {
            LoadAssetBundle(directory, folderObjName);
            
            mUntrackedAssetBundles = new Dictionary<string, ABData>();
            mUploadedAssetBundles = new Dictionary<Hash128, ABData>();

            // Todo: Load dictionary of uploaded asset bundles
            // Todo: Load list of new asset bundles that aren't in the 'uploaded asset bundles dictionary'
        }

        public void Refresh()
        {
        }

        public void CleanUp()
        {
            if (mFolderObj)
            {
                mFolderObj.Unload(true);   
                SaveUploadedAssetBundlesList();
            }
            mManifest = null;
        }
        
        public void LoadUI(VisualElement root)
        {
            mHelpBoxHolder = root.Query<Box>("helpbox-holder").First();

            // Asset Bundle Entries
            mUntrackedAssetBundleHolder = root.Query<Box>("new-entries").First();
            ReloadUntrackedAssetBundles();

            // var generateAssetBundles = root.Query<Button>("create-asset-bundles-button").First();
            // generateAssetBundles.clicked += () => {
            //     CreateAssetBundles.BuildAllAssetBundles(mRawrshakSettings.buildTarget, mRawrshakSettings.assetBundleFolder);
            //     Refresh();
            // };

            // var walletInfoBox = root.Query<Box>("wallet-info").First();
            // SerializedObject so = new SerializedObject(mArweaveSettings);
            // walletInfoBox.Bind(so);

            // var refreshBalanceButton = root.Query<Button>("refresh-balance").First();
            // refreshBalanceButton.clicked += () => {
            //     mArweaveSettings.RefreshBalance();
            // };

            // mAssetBundleEntries = root.Query<Box>("asset-bundle-entries").First();
            // mUploadedAsssetBundleEntries = root.Query<Box>("uploaded-asset-bundle-entries").First();
            // mAssetBundleInfoBox = root.Query<Box>("asset-bundle-info").First();

            // // var printButton = root.Query<Button>("print-button").First();
            // // printButton.clicked += () => {
            // //     Refresh();
            // // };
            
            // var uploadButton = root.Query<Button>("upload-button").First();
            // uploadButton.clicked += () => {
            //     UploadAssetBundles();

            //     // Update UI
            //     Refresh();
            //     RefreshUploadedAssetBundlesBox();
            // };

            // // Refresh some UI
            // Refresh();
            // RefreshUploadedAssetBundlesBox();

            // if (String.IsNullOrEmpty(mArweaveSettings.wallet))
            // {
            //     HelpBox helpbox = new HelpBox("Load an Arweave Wallet in the Settings tab.", HelpBoxMessageType.Error);
            //     mHelpBoxHolder.Add(helpbox);
            // }
        }
        
        public void ReloadUntrackedAssetBundles()
        {
            // Clear helper box
            mHelpBoxHolder.Clear();

            // Load Entry UML
            var entry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetBundleMenu/UntrackedAssetBundle.uxml");

            if (mManifest)
            {
                var bundleNames = mManifest.GetAllAssetBundles();
                
                // mark all bundles for delete
                foreach(var bundle in mUntrackedAssetBundles)
                {
                    bundle.Value.mMarkedForDelete = true;
                }

                foreach(string name in bundleNames)
                {
                    var hash = mManifest.GetAssetBundleHash(name);
                    // Debug.Log("AssetBundle: " + name + ", hash: " + hash);

                    // Todo: Skip if it is already uploaded
                    // if (mAssetBundlesInfo.mDictionary.ContainsKey(hash))
                    // {
                    //     continue;
                    // }
                    
                    if (mUntrackedAssetBundles.ContainsKey(name))
                    {
                        Debug.Log("Replacing name: " + name);
                        var bundle = mUntrackedAssetBundles[name];
                        bundle.mHashId = hash;
                        bundle.mHash = hash.ToString();
                        bundle.mSelectedForUploading = false;
                        bundle.mVisualElement.contentContainer.Query<Label>("asset-bundle-hash").First().text = hash.ToString();
                        bundle.mMarkedForDelete = false;
                    }
                    else
                    {
                        // find or add the asset bundle in the new asset bundle lists
                        ABData bundle = new ABData(hash, name);
                        mUntrackedAssetBundles.Add(name, bundle);

                        // Add entry to UI
                        TemplateContainer entryTree = entry.CloneTree();
                        entryTree.contentContainer.Query<Label>("asset-bundle-name").First().text = name;
                        entryTree.contentContainer.Query<Label>("asset-bundle-hash").First().text = hash.ToString();

                        // Set Toggle Callback
                        var selectedToggle = entryTree.contentContainer.Query<Toggle>("asset-bundle-selected").First();
                        selectedToggle.RegisterCallback<ChangeEvent<bool>>((evt) => {
                            bundle.mSelectedForUploading = (evt.target as Toggle).value;
                        });
                        
                        bundle.mVisualElement = entryTree;

                        // Add entry to UI
                        mUntrackedAssetBundleHolder.Add(entryTree);
                    }

                    // // Select Asset Bundle Callback
                    // entryTree.RegisterCallback<MouseDownEvent>((evt) => {
                    //     // assetBundleData.selected = (evt.target as Label).value;
                    //     Debug.Log("Info to Display: " + assetBundleData.mName);
                    //     ViewAssetBundleInfo(assetBundleData);
                    // });
                }

                // Delete bundles marked for delete
                List<string> bundlesToDelete = new List<string>();
                foreach(var bundle in mUntrackedAssetBundles)
                {
                    if (bundle.Value.mMarkedForDelete)
                    {
                        mUntrackedAssetBundleHolder.Remove(bundle.Value.mVisualElement);
                        bundlesToDelete.Add(bundle.Key);
                    }
                }

                foreach(var name in bundlesToDelete)
                {
                    mUntrackedAssetBundles.Remove(name);
                }
            }
            else
            {
                Debug.Log("Manifest Doesn't exist!");
                mUntrackedAssetBundleHolder.Clear();
                mUntrackedAssetBundles.Clear();
            }

            Debug.Log("New Asset Bundle Size: " + mUntrackedAssetBundles.Count);
        }




        // public void UploadAssetBundles()
        // {
        //     // Get the selected Asset Bundles
        //     var iter = mNewAssetBundles.GetEnumerator();
        //     List<string> bundlesToUpload = new List<string>();
        //     while(iter.MoveNext())
        //     {
        //         if (iter.Current.Value.mSelectedForUploading)
        //         {
        //             Debug.Log(mAssetBundlePath + iter.Current.Value.mName);
        //             bundlesToUpload.Add(iter.Current.Key);
        //         }
        //     }
        //     Debug.Log("Dictionary Size left: " + mNewAssetBundles.Count);

        //     // Upload the asset bundles to storage
        //     var uploadIter = bundlesToUpload.GetEnumerator();
        //     while (uploadIter.MoveNext())
        //     {
        //         var bundle = mNewAssetBundles[uploadIter.Current];
        //         if (mAssetBundlesInfo.mDictionary.ContainsKey(bundle.mHashId))
        //         {
        //             // ignore; Don't need to upload what has already been uploaded.
        //             continue;
        //         }

        //         // Upload to storage
        //         mArweaveSettings.bundleForUpload = bundle;
        //         mArweaveSettings.UploadAssetBundle();

        //         // Remove from new asset bundles list
        //         mNewAssetBundles.Remove(uploadIter.Current);

        //         // Update Asset Bundle Data
        //         bundle.mSelectedForUploading = false;
        //         bundle.mUploadedTimestamp = DateTime.Now.ToString();
                
        //         // Add to mAssetBundlesInfo
        //         mAssetBundlesInfo.mDictionary.Add(bundle.mHashId, bundle);
                
        //         ViewAssetBundleInfo(bundle);
        //     }
        // }

        // public void RefreshUploadedAssetBundlesBox()
        // {
        //     // clear all entries first
        //     mUploadedAsssetBundleEntries.Clear();

        //     // Load Entry UML
        //     var entry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/UploadedAssetBundleEntry.uxml");

        //     // Upload Asset Bundle Box with new entries
        //     var iter = mAssetBundlesInfo.mDictionary.GetEnumerator();
        //     while (iter.MoveNext())
        //     {
        //         var bundle = iter.Current.Value;
        //         Debug.Log("Uploaded Asset Bundle: " + bundle.mName);
                
        //         TemplateContainer entryTree = entry.CloneTree();
        //         entryTree.contentContainer.Query<Label>("asset-bundle-name").First().text = bundle.mName;
        //         entryTree.contentContainer.Query<Label>("asset-bundle-uri").First().text = bundle.mHash;
                
        //         mUploadedAsssetBundleEntries.Add(entryTree);
                
        //         // Select Asset Bundle Callback to show info
        //         entryTree.RegisterCallback<MouseDownEvent>((evt) => {
        //             // assetBundleData.selected = (evt.target as Label).value;
        //             Debug.Log("Info to Display: " + bundle.mName);
        //             // Todo: Show Info
        //             ViewAssetBundleInfo(bundle);
        //         });
        //     }

        //     SaveUploadedAssetBundlesList();
        // }

        // private void ViewAssetBundleInfo(AssetBundleData bundle)
        // {
        //     mAssetBundleInfoBox.Clear();
        //     var entry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetBundleDetails.uxml");
        //     TemplateContainer entryTree = entry.CloneTree();

        //     entryTree.contentContainer.Query<TextField>("name").First().value = bundle.mName;
        //     entryTree.contentContainer.Query<TextField>("name").First().SetEnabled(false);
        //     entryTree.contentContainer.Query<TextField>("hash").First().value = bundle.mHash;
        //     entryTree.contentContainer.Query<TextField>("hash").First().SetEnabled(false);
        //     entryTree.contentContainer.Query<TextField>("transaction-id").First().value = bundle.mTransactionId;
        //     entryTree.contentContainer.Query<TextField>("transaction-id").First().SetEnabled(false);
        //     entryTree.contentContainer.Query<TextField>("uri").First().value = bundle.mUri;
        //     entryTree.contentContainer.Query<TextField>("uri").First().SetEnabled(false);
        //     entryTree.contentContainer.Query<TextField>("uploaded-timestamp").First().value = bundle.mUploadedTimestamp;
        //     entryTree.contentContainer.Query<TextField>("uploaded-timestamp").First().SetEnabled(false);
        //     entryTree.contentContainer.Query<TextField>("status").First().value = bundle.mStatus;
        //     entryTree.contentContainer.Query<TextField>("status").First().SetEnabled(false);

        //     var refreshButton = entryTree.contentContainer.Query<Button>("check-status").First();
        //     refreshButton.clicked += () => {
        //         mArweaveSettings.bundleForUpload = bundle;
        //         mArweaveSettings.CheckStatus();
        //         ViewAssetBundleInfo(bundle);
        //     };
            
        //     mAssetBundleInfoBox.Add(entryTree);
        // }

        public void LoadAssetBundle(string directory, string folderObjName)
        {
            mAssetBundleDirectory = directory;
            Debug.Log("Load Asset Bundle: " + directory);
            if (mFolderObj != null)
            {
                mFolderObj.Unload(true);  
                mManifest = null; 
            }

            if (File.Exists("Assets/" + mAssetBundleDirectory + "/" + folderObjName))
            {
                Debug.Log("Folder Exists! " + mAssetBundleDirectory + "/" + folderObjName);
                mFolderObj = AssetBundle.LoadFromFile(Application.dataPath + "/" + mAssetBundleDirectory + "/" + folderObjName);
                mManifest = mFolderObj.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
        }

        public void SaveUploadedAssetBundlesList()
        {
            Debug.Log("AssetBundlesInfo length: " + mUploadedAssetBundles.Count);
            if (mUploadedAssetBundles == null) {
                return;
            }

            // Convert List of Uploaded Asset Bundles to json file
            List<ABData> dataList = new List<ABData>();
            var iter = mUploadedAssetBundles.GetEnumerator();
            while (iter.MoveNext()) 
            {
                dataList.Add(iter.Current.Value);
            }
            string data = JsonUtility.ToJson(dataList);
            
            // Delete file and .meta filefirst
            File.Delete(UPLOADED_LIST_FILE);
            File.Delete(UPLOADED_LIST_FILE + ".meta");
            
            // write file 
            StreamWriter writer = new StreamWriter(UPLOADED_LIST_FILE, true);
            writer.WriteLine(data);
            writer.Close();
            AssetDatabase.Refresh();
        }

        public void AddErrorHelpbox(string errorMsg)
        {
            mHelpBoxHolder.Add(new HelpBox(errorMsg, HelpBoxMessageType.Error));
        }
    }

}