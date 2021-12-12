using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
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
        // Singleton instance
        static ABManager _instance = null;
        
        // Static Properties

        // Private Properties
        private string mAssetBundleDirectory; // Asset Bundle Directory
        private AssetBundle mFolderObj; // Asset Bundle Folder Object
        private AssetBundleManifest mManifest; // Asset Bundle Folder Manifest Object
        private UnityEvent<ABData> bundleSelected = new UnityEvent<ABData>();
        private SupportedBuildTargets mCurrentBuildTarget;
        private float mEstimatedTotalCost;

        Dictionary<string, ABData> mUntrackedAssetBundles;
        
        // UI
        Box mUntrackedAssetBundleHolder;
        Box mHelpBoxHolder;
        Label mEstimatedTotalCostLabel;
        VisualTreeAsset mUntrackedBundleEntry;


        public static ABManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<ABManager>();
                if (!_instance)
                {
                    _instance = ScriptableObject.CreateInstance<ABManager>();
                    
                    _instance.mCurrentBuildTarget = AssetBundleMenuConfig.Instance.buildTarget;
                    _instance.LoadAssetBundle(AssetBundleMenuConfig.Instance.assetBundleFolder, AssetBundleMenuConfig.Instance.buildTarget);
                        
                }
                return _instance;
            }
        }

        public void OnEnable()
        {
            mUntrackedAssetBundles = new Dictionary<string, ABData>();
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
        
        public void LoadUI(VisualElement root)
        {
            mHelpBoxHolder = root.Query<Box>("helpbox-holder").First();

            // Asset Bundle Entries
            mUntrackedAssetBundleHolder = root.Query<Box>("new-entries").First();
            
            // Load Entry UXMLs
            mUntrackedBundleEntry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Rawrshak/Editor/UXML/AssetBundleMenu/UntrackedAssetBundle.uxml");

            mEstimatedTotalCostLabel = root.Query<Label>("estimated-cost").First();
            
            ReloadUntrackedAssetBundles();
        }
        
        public void ReloadUntrackedAssetBundles()
        {
            // Clear helper box
            mHelpBoxHolder.Clear();
            mUntrackedAssetBundleHolder.Clear();
            mUntrackedAssetBundles.Clear();

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
                        bundle.mBuildTarget = mCurrentBuildTarget;
                        bundle.UpdateAssetNames();
                        bundle.UpdateFileSize();
                        
                        Debug.Log("Already Added: " + bundle.mName);
                    }
                    else
                    {
                        // find or add the asset bundle in the new asset bundle lists
                        string fileLocation = Application.dataPath + "/" + mAssetBundleDirectory + "/" + name;
                        ABData bundle = ABData.CreateInstance(hash, name, fileLocation, mCurrentBuildTarget);
                        mUntrackedAssetBundles.Add(name, bundle);
                    }
                    
                    // Add entry to UI
                    var addedBundle = mUntrackedAssetBundles[name];

                    TemplateContainer entryTree = mUntrackedBundleEntry.CloneTree();
                    entryTree.contentContainer.Query<Label>("asset-bundle-name").First().text = name;
                    entryTree.contentContainer.Query<Label>("asset-bundle-hash").First().text = hash.ToString();

                    // Set Toggle Callback
                    var selectedToggle = entryTree.contentContainer.Query<Toggle>("asset-bundle-selected").First();
                    selectedToggle.RegisterCallback<ChangeEvent<bool>>((evt) => {
                        addedBundle.mSelectedForUploading = (evt.target as Toggle).value;

                        // If Bundle is selected, get the estimated cost and add it to total cost;
                        EditorCoroutineUtility.StartCoroutine(UpdateEstimatedCost(addedBundle, addedBundle.mSelectedForUploading), this);
                    });

                    // Select Asset Bundle Callback to show info
                    entryTree.RegisterCallback<MouseDownEvent>((evt) => {
                        bundleSelected.Invoke(addedBundle);;
                    });
                    
                    addedBundle.mVisualElement = entryTree;

                    // Add entry to UI
                    Debug.Log("Adding Untracked Asset Bundle: " + addedBundle.mName);
                    mUntrackedAssetBundleHolder.Add(entryTree);
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
            }

            Debug.Log("New Asset Bundle Size: " + mUntrackedAssetBundles.Count);
        }

        public void LoadAssetBundle(string directory, SupportedBuildTargets builtTarget)
        {
            mAssetBundleDirectory = directory;
            mCurrentBuildTarget = builtTarget;
            Debug.Log("Load Asset Bundle: " + directory);
            if (mFolderObj != null)
            {
                mFolderObj.Unload(true);  
                mManifest = null; 
            }
            
            string folderObjName = builtTarget.ToString();
            if (File.Exists("Assets/" + mAssetBundleDirectory + "/" + folderObjName))
            {
                Debug.Log("Folder Exists: Assets/" + mAssetBundleDirectory + "/" + folderObjName);
                mFolderObj = AssetBundle.LoadFromFile(Application.dataPath + "/" + mAssetBundleDirectory + "/" + folderObjName);
                mManifest = mFolderObj.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
        }

        public void AddErrorHelpbox(string errorMsg)
        {
            mHelpBoxHolder.Add(new HelpBox(errorMsg, HelpBoxMessageType.Error));
        }

        private IEnumerator UpdateEstimatedCost(ABData bundle, bool isSelected)
        {
            // Todo: Implement this when you have the Estimate Cost API
            mEstimatedTotalCost += (isSelected) ? 10.0f : -10.0f;
            mEstimatedTotalCostLabel.text = String.Format("{0} AR", mEstimatedTotalCost.ToString("n2"));
            yield return null;
        }
    }
}