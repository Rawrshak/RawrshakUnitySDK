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
        static string UPLOADED_LIST_FILE = "Assets/Editor/Resources/AssetBundlesMenuConfig/UploadedListAssetBundlesInfo.json";

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
            
            mUntrackedAssetBundles = new Dictionary<string, ABData>();
            mUploadedAssetBundles = new Dictionary<Hash128, ABData>();

            // Todo: Load dictionary of uploaded asset bundles
            // Todo: Load list of new asset bundles that aren't in the 'uploaded asset bundles dictionary'
        }

        public void SetBundleSelectedCallback(UnityAction<ABData> bundleSelectedCallback)
        {
            bundleSelected.AddListener(bundleSelectedCallback);
        }

        public void SetUploadBundleCallback(UnityAction<List<ABData>> uploadBundleCallback)
        {
            mUploadBundleCallback.AddListener(uploadBundleCallback);
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

            var uploadButton = root.Query<Button>("upload-button").First();
            uploadButton.clicked += () => {
                var list = BuildUploadList();
                mUploadBundleCallback.Invoke(list);
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

                    // Todo: Skip if it is already uploaded
                    // if (mAssetBundlesInfo.mDictionary.ContainsKey(hash))
                    // {
                    //     continue;
                    // }
                    
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
        
        private List<ABData> BuildUploadList()
        {
            var list = new List<ABData>();

            var iter = mUntrackedAssetBundles.GetEnumerator();
            while(iter.MoveNext())
            {
                if (iter.Current.Value.mSelectedForUploading)
                {
                    list.Add(iter.Current.Value);
                }
            }

            return list;
        }
    }
}