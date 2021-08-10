using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Events;
using System.IO;

namespace Rawrshak
{
    public class ABManager : ScriptableObject
    {
        // Static Properties
        static string ASSET_BUNDLES_UPLOADED_DIRECTORY = "UploadedAssetBundles";

        // Private Properties
        private string mAssetBundleDirectory; // Asset Bundle Directory
        private AssetBundle mFolderObj; // Asset Bundle Folder Object
        private AssetBundleManifest mManifest; // Asset Bundle Folder Manifest Object
        private UnityEvent<ABData> bundleSelected = new UnityEvent<ABData>();
        private UnityEvent<List<ABData>> mUploadBundleCallback = new UnityEvent<List<ABData>>();

        Dictionary<string, ABData> mUntrackedAssetBundles;
        Dictionary<Hash128, ABData> mUploadedAssetBundles;
        
        // UI
        Box mUntrackedAssetBundleHolder;
        Box mHelpBoxHolder;

        public void Init(string directory, string folderObjName)
        {
            LoadAssetBundle(directory, folderObjName);

            // Todo: Load dictionary of uploaded asset bundles
            // Todo: Load list of new asset bundles that aren't in the 'uploaded asset bundles dictionary'
        }

        public void OnEnable()
        {            
            mUntrackedAssetBundles = new Dictionary<string, ABData>();
            mUploadedAssetBundles = new Dictionary<Hash128, ABData>();

            // Create Uploaded Asset Bundles Directory if necessary
            string directoryPath = String.Format("{0}/{1}", AssetBundleMenu.RESOURCES_FOLDER, ASSET_BUNDLES_UPLOADED_DIRECTORY);
            if(!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Load all Asset Bundle Data from storage if any
            foreach (ABData bundle in Resources.LoadAll("UploadedAssetBundles", typeof(ABData)))
            {
                // Only Load asset bundle data that are stored
                if (EditorUtility.IsPersistent(bundle))
                {
                    bundle.SetHash128();

                    // Set Non-serialized data to default values
                    if (!mUploadedAssetBundles.ContainsKey(bundle.mHashId))
                    {
                        mUploadedAssetBundles.Add(bundle.mHashId, bundle);
                        Debug.Log("Adding Uploaded Bundle: " + bundle.mName + ", ID: " + bundle.mHash);
                    }
                }
            }
        }

        public void OnDisable()
        {
            if (mFolderObj)
            {
                mFolderObj.Unload(true);
            }
            mManifest = null;

            // Save any asset bundles whose data changed
            AssetDatabase.SaveAssets();
            Debug.Log("ABManager OnDisable");
        }

        public void SetBundleSelectedCallback(UnityAction<ABData> bundleSelectedCallback)
        {
            bundleSelected.AddListener(bundleSelectedCallback);
        }

        public void SetUploadBundleCallback(UnityAction<List<ABData>> uploadBundleCallback)
        {
            mUploadBundleCallback.AddListener(uploadBundleCallback);
        }
        
        public void LoadUI(VisualElement root)
        {
            mHelpBoxHolder = root.Query<Box>("helpbox-holder").First();

            // Asset Bundle Entries
            mUntrackedAssetBundleHolder = root.Query<Box>("new-entries").First();

            var uploadButton = root.Query<Button>("upload-button").First();
            uploadButton.clicked += () => {
                // Todo: Get Config a better way
                var config = Resources.FindObjectsOfTypeAll(typeof(UploadConfig));
                if (config[0] != null)
                {
                    if (String.IsNullOrEmpty(((UploadConfig)config[0]).walletAddress))
                    {
                        AddErrorHelpbox("No Wallet Loaded.");
                        return;
                    }
                    var list = BuildUploadList();
                    mUploadBundleCallback.Invoke(list);
                }
            };
            
            ReloadUntrackedAssetBundles();
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

                    if (mUploadedAssetBundles.ContainsKey(hash))
                    {
                        continue;
                    }
                    
                    if (mUntrackedAssetBundles.ContainsKey(name))
                    {
                        var bundle = mUntrackedAssetBundles[name];
                        bundle.mHashId = hash;
                        bundle.mHash = hash.ToString();
                        bundle.mSelectedForUploading = false;
                        bundle.mFileLocation = Application.dataPath + "/" + mAssetBundleDirectory + "/" + name;
                        bundle.mVisualElement.contentContainer.Query<Label>("asset-bundle-hash").First().text = hash.ToString();
                        bundle.mVisualElement.contentContainer.Query<Toggle>("asset-bundle-selected").First().value = false;
                        bundle.mMarkedForDelete = false;
                        bundle.UpdateAssetNames();
                        bundle.UpdateFileSize();
                    }
                    else
                    {
                        // find or add the asset bundle in the new asset bundle lists
                        // Todo: Update this to include file location
                        string fileLocation = Application.dataPath + "/" + mAssetBundleDirectory + "/" + name;
                        ABData bundle = new ABData(hash, name, fileLocation);
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

                        // Select Asset Bundle Callback to show info
                        entryTree.RegisterCallback<MouseDownEvent>((evt) => {
                            bundleSelected.Invoke(bundle);;
                        });
                        
                        bundle.mVisualElement = entryTree;

                        // Add entry to UI
                        mUntrackedAssetBundleHolder.Add(entryTree);
                    }
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

        public void AddErrorHelpbox(string errorMsg)
        {
            mHelpBoxHolder.Add(new HelpBox(errorMsg, HelpBoxMessageType.Error));
        }
        
        private List<ABData> BuildUploadList()
        {
            var list = new List<ABData>();

            var iter = mUntrackedAssetBundles.GetEnumerator();
            while(iter.MoveNext())
            {
                if (iter.Current.Value.mSelectedForUploading)
                {
                    list.Add(iter.Current.Value);
                    
                    // Save uploaded bundle to file
                    SaveAssetBundle(iter.Current.Value);

                    // Add uploaded bundle to dictionary
                    mUploadedAssetBundles.Add(iter.Current.Value.mHashId, iter.Current.Value);
                }
            }
            AssetDatabase.SaveAssets();

            // remove mUntrackedAssetBundles
            foreach (var bundle in list)
            {
                mUntrackedAssetBundleHolder.Remove(bundle.mVisualElement);
                mUntrackedAssetBundles.Remove(bundle.mName);
            }

            Debug.Log("Untracked Bundles List Size: " + mUntrackedAssetBundles.Count);

            return list;
        }

        private void SaveAssetBundle(ABData bundle)
        {
            string storageFile = String.Format(
                    "{0}/{1}/{2}-{3}.asset",
                    AssetBundleMenu.RESOURCES_FOLDER,
                    ASSET_BUNDLES_UPLOADED_DIRECTORY, 
                    bundle.mName, 
                    bundle.mHash);

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(storageFile);
            if (!fileInfo.Exists)
            {
                AssetDatabase.CreateAsset(bundle, storageFile);
            }
        }
    }
}