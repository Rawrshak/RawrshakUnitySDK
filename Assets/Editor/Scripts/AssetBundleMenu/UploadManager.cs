
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEditor.Scripting.Python;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Rawrshak
{
    public class UploadManager : ScriptableObject
    {
        public ABData bundleForUpload;
        public ABData bundleToCheckStatus;

        // Private Properties
        public UploadConfig mConfig;
        
        Box mHelpBoxHolder;

        // Static Properties
        static string UPLOAD_CONFIG_FILE = "UploadConfig";

        public void OnEnable()
        {
            Debug.Log("Initializing UploadManager.");
            mConfig = Resources.Load<UploadConfig>(String.Format("{0}/{1}", AssetBundleMenu.ASSET_BUNDLES_MENU_CONFIG_DIRECTORY, UPLOAD_CONFIG_FILE));
            if (mConfig == null)
            {
                mConfig = ScriptableObject.CreateInstance<UploadConfig>();
                mConfig.Init();
                AssetDatabase.CreateAsset(mConfig, String.Format("{0}/{1}/{2}.asset", AssetBundleMenu.RESOURCES_FOLDER, AssetBundleMenu.ASSET_BUNDLES_MENU_CONFIG_DIRECTORY, UPLOAD_CONFIG_FILE));
            }
            AssetDatabase.SaveAssets();
        }

        public void LoadUI(VisualElement root)
        {
            mHelpBoxHolder = root.Query<Box>("helpbox-holder").First();

            var uploadSettingsFoldout = root.Query<Foldout>("upload-settings").First();
            SerializedObject so = new SerializedObject(mConfig);
            uploadSettingsFoldout.Bind(so);

            var refreshBalanceButton = root.Query<Button>("refresh-balance-button").First();
            refreshBalanceButton.clicked += () => {
                EditorCoroutineUtility.StartCoroutine(RefreshBalance(), this);
            };
            
            var loadWalletButton = root.Query<Button>("load-wallet-button").First();
            loadWalletButton.clicked += () => {
                EditorCoroutineUtility.StartCoroutine(LoadWallet(), this);
            };
            
            // var uploadButton = root.Query<Button>("upload-button").First();
            // uploadButton.clicked += () => {
            //     UploadAssetBundles();

            //     // Update UI
            //     Refresh();
            //     RefreshUploadedAssetBundlesBox();
            // };

            if (String.IsNullOrEmpty(mConfig.walletAddress))
            {
                HelpBox helpbox = new HelpBox("Load an Arweave Wallet to upload asset bundles.", HelpBoxMessageType.Error);
                mHelpBoxHolder.Add(helpbox);
            }
        }

        public void UploadBundles(List<ABData> bundles)
        {
            if (String.IsNullOrEmpty(mConfig.walletAddress))
            {
                AddErrorHelpbox("No Wallet Loaded.");
                return;
            }

            foreach(var bundle in bundles)
            {
                bundleForUpload = bundle;
                Debug.Log("Uploading bundle: " + bundleForUpload.mName);
                
                // This has to be synchronous because the python script reads 
                // off of the bundle listed here. You might want to just have
                // the python script load the entire bundle list and upload instead
                // of doing it one by one. At which point, you can rever this back
                // to a coroutine
                // Todo: Update script to upload the entire list of bundle instead
                // of doing this one by one. 
                UploadAssetBundle();
                // EditorCoroutineUtility.StartCoroutine(UploadAssetBundle(), this);
            }
        }

        public void CheckUploadStatus(ABData bundle)
        {
            bundleToCheckStatus = bundle;
            
            // check status
            EditorCoroutineUtility.StartCoroutine(CheckStatus(), this);
        }
        
        IEnumerator LoadWallet()
        {
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/VerifyArweaveConnection.py");
            yield return null;
        }
        
        IEnumerator RefreshBalance()
        {
            if (String.IsNullOrEmpty(mConfig.walletAddress))
            {
                AddErrorHelpbox("No Wallet Loaded.");
                yield break;
            }
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/RefreshWalletBalance.py");
            yield return null;
        }

        // Todo: rever this back to a coroutine
        void UploadAssetBundle()
        {
            if (String.IsNullOrEmpty(mConfig.walletAddress))
            {
                AddErrorHelpbox("No Wallet Loaded.");
                // yield break;
            }
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/UploadAssetBundle.py");
            // yield return null;

            // Save the info that the python scripts updated
            EditorUtility.SetDirty(bundleForUpload);
            AssetDatabase.SaveAssets();
        }

        IEnumerator CheckStatus()
        {
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/CheckStatus.py");
            yield return null;
        }

        public void AddErrorHelpbox(string errorMsg)
        {
            mHelpBoxHolder.Add(new HelpBox(errorMsg, HelpBoxMessageType.Error));
        }
    }
}
