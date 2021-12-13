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
    public class AssetBundleManager : ScriptableObject
    {
        // Singleton instance
        static AssetBundleManager _instance = null;
        
        // Static Properties

        // Private Properties
        private string mAssetBundleDirectory; // Asset Bundle Directory
        private AssetBundle mFolderObj; // Asset Bundle Folder Object
        private AssetBundleManifest mManifest; // Asset Bundle Folder Manifest Object
        private UnityEvent<AssetBundleData> bundleSelected = new UnityEvent<AssetBundleData>();
        private SupportedBuildTargets mCurrentBuildTarget;
        private float mEstimatedTotalCost;

        Dictionary<string, AssetBundleData> mUntrackedAssetBundles;
        
        // UI
        Box mUntrackedAssetBundleHolder;
        Box mHelpBoxHolder;
        Label mTotalStorage;
        VisualTreeAsset mUntrackedBundleEntry;


        public static AssetBundleManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<AssetBundleManager>();
                if (!_instance)
                {
                    _instance = ScriptableObject.CreateInstance<AssetBundleManager>();
                    
                    _instance.mCurrentBuildTarget = AssetBundleMenuConfig.Instance.selectedBuildTarget;
                    _instance.LoadAssetBundle(AssetBundleMenuConfig.Instance.selectedTargetBuildAssetBundleFolder, AssetBundleMenuConfig.Instance.selectedBuildTarget);
                        
                }
                return _instance;
            }
        }

        public void OnEnable()
        {
            mUntrackedAssetBundles = new Dictionary<string, AssetBundleData>();
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
            Debug.Log("AssetBundleManager OnDisable");
        }

        public void SetBundleSelectedCallback(UnityAction<AssetBundleData> bundleSelectedCallback)
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

            mTotalStorage = root.Query<Label>("total-storage").First();
            var calculatorButton = root.Query<Button>("calculator").First();
            calculatorButton.clicked += () => {
                Application.OpenURL("https://prices.ardrive.io/");
            };
            
            LoadAssetBundle(AssetBundleMenuConfig.Instance.selectedTargetBuildAssetBundleFolder, AssetBundleMenuConfig.Instance.selectedBuildTarget);
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

                foreach(string name in bundleNames)
                {
                    var hash = mManifest.GetAssetBundleHash(name);
                    // Debug.Log("AssetBundle: " + name + ", hash: " + hash);
                    
                    if (mUntrackedAssetBundles.ContainsKey(name))
                    {
                        var bundle = mUntrackedAssetBundles[name];
                        bundle.mHashId = hash;
                        bundle.mHash = hash.ToString();
                        bundle.mSelected = false;
                        bundle.mFileLocation = Application.dataPath + "/" + mAssetBundleDirectory + "/" + name;
                        bundle.mVisualElement.contentContainer.Query<Label>("asset-bundle-hash").First().text = hash.ToString();
                        bundle.mVisualElement.contentContainer.Query<Toggle>("asset-bundle-selected").First().value = false;
                        bundle.mBuildTarget = mCurrentBuildTarget;
                        bundle.UpdateAssetNames();
                        bundle.UpdateFileSize();
                        
                        Debug.Log("Already Added: " + bundle.mName);
                    }
                    else
                    {
                        // find or add the asset bundle in the new asset bundle lists
                        string fileLocation = Application.dataPath + "/" + mAssetBundleDirectory + "/" + name;
                        AssetBundleData bundle = AssetBundleData.CreateInstance(hash, name, fileLocation, mCurrentBuildTarget);
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
                        addedBundle.mSelected = (evt.target as Toggle).value;

                        // If Bundle is selected, get the estimated cost and add it to total cost;
                        EditorCoroutineUtility.StartCoroutine(UpdateEstimatedCost(addedBundle, addedBundle.mSelected), this);
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
            Debug.Log("Folder Exists: Assets/" + mAssetBundleDirectory + "/" + folderObjName);
            if (File.Exists("Assets/" + mAssetBundleDirectory + "/" + folderObjName))
            {
                mFolderObj = AssetBundle.LoadFromFile(Application.dataPath + "/" + mAssetBundleDirectory + "/" + folderObjName);
                mManifest = mFolderObj.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
        }

        public void AddErrorHelpbox(string errorMsg)
        {
            mHelpBoxHolder.Add(new HelpBox(errorMsg, HelpBoxMessageType.Error));
        }

        public void ResetCalculator()
        {
            mEstimatedTotalCost = 0.0f;
            mTotalStorage.text = String.Format("{0}", mEstimatedTotalCost.ToString("n2"));
        }

        private IEnumerator UpdateEstimatedCost(AssetBundleData bundle, bool isSelected)
        {
            mEstimatedTotalCost += ((isSelected) ? bundle.mFileSize : -bundle.mFileSize) / 1000.0f;
            mTotalStorage.text = String.Format("{0}", mEstimatedTotalCost.ToString("n2"));
            yield return null;
        }
    }
}