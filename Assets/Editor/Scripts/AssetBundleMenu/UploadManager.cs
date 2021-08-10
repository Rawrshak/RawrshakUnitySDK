
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

        public void Init()
        {
            Debug.Log("Initializing ArweaveSettings.");
            if (mConfig == null)
            {
                mConfig = ScriptableObject.CreateInstance<UploadConfig>();
                mConfig.Init();
            }
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
                // Todo: Upload files
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

        IEnumerator UploadAssetBundle()
        {
            if (String.IsNullOrEmpty(mConfig.walletAddress))
            {
                AddErrorHelpbox("No Wallet Loaded.");
                yield break;
            }
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/UploadAssetBundle.py");
            yield return null;
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
