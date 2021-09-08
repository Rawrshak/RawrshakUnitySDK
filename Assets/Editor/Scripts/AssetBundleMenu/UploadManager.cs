using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Networking;

namespace Rawrshak
{
    public class UploadManager : ScriptableObject
    {
        static string uploadBaseUrl = "https://dwsserver-rx5q3vrrpa-uk.a.run.app/api/bucket";
        public ABData bundleForUpload;
        public ABData bundleToCheckStatus;

        // Private Properties
        public UploadConfig mConfig;
        
        Box mHelpBoxHolder;

        // Static Properties
        static string UPLOAD_CONFIG_FILE = "UploadConfig";

        public static UploadManager CreateInstance()
        {
            return ScriptableObject.CreateInstance<UploadManager>();
        }

        public void OnEnable()
        {
            Debug.Log("Initializing UploadManager.");
            mConfig = Resources.Load<UploadConfig>(String.Format("{0}/{1}", AssetBundleMenu.ASSET_BUNDLES_MENU_CONFIG_DIRECTORY, UPLOAD_CONFIG_FILE));
            if (mConfig == null)
            {
                mConfig = UploadConfig.CreateInstance();
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
            
            if (String.IsNullOrEmpty(mConfig.walletAddress))
            {
                HelpBox helpbox = new HelpBox("Load an Arweave Wallet to upload asset bundles.", HelpBoxMessageType.Error);
                mHelpBoxHolder.Add(helpbox);
            }
        }

        public void UploadBundles(List<ABData> bundles)
        {
            if (String.IsNullOrEmpty(mConfig.dwsApiKey))
            {
                AddErrorHelpbox("No DWS API Key Set");
                return;
            }

            if (String.IsNullOrEmpty(mConfig.dwsBucketName))
            {
                AddErrorHelpbox("No DWS Bucket Name Set");
                return;
            }
            
            EditorCoroutineUtility.StartCoroutine(UploadAssetBundle(bundles), this);
        }

        public void CheckUploadStatus(ABData bundle)
        {
            bundleToCheckStatus = bundle;
            
            // check status
            EditorCoroutineUtility.StartCoroutine(CheckStatus(), this);
        }

        IEnumerator CheckStatus()
        {
            // Todo: Implement this when you have the Wallet API
            yield return null;
        }
        
        IEnumerator LoadWallet()
        {
            // Todo: Implement this when you have the Wallet API
            yield return null;
        }
        
        IEnumerator RefreshBalance()
        {
            // Todo: Implement this when you have the Wallet API
            yield return null;
        }

        IEnumerator UploadAssetBundle(List<ABData> bundles)
        {
            foreach(var bundle in bundles)
            {
                Debug.Log("Uploading bundle: " + bundle.mName);

                string filePath = bundle.mName;
                if (!String.IsNullOrEmpty(mConfig.dwsFolderPath))
                {
                    filePath = string.Format("{0}/{1}", mConfig.dwsFolderPath, filePath);
                }
                string uri = String.Format("{0}/{1}/{2}", uploadBaseUrl, mConfig.dwsBucketName, filePath);

                Debug.Log("Uri: " + uri);

                // Upload the file
                using (var uwr = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST))
                {
                    // Set API key header
                    uwr.SetRequestHeader("X-APIKey", mConfig.dwsApiKey);

                    // Create Upload File Handler and set it to the file location
                    uwr.uploadHandler = new UploadHandlerFile(bundle.mFileLocation);
                    uwr.uploadHandler.contentType = "application/octet-stream";

                    uwr.downloadHandler = new DownloadHandlerBuffer();

                    // Send unity request
                    UnityWebRequestAsyncOperation request = uwr.SendWebRequest();
                    
                    float progress = 0.0f;
                    while (!request.isDone)
                    {
                        if (progress < request.progress)
                        {
                            progress = request.progress;
                            Debug.Log(request.progress);
                        }
                        yield return null;
                    }
                    Debug.Log("Upload Finished!");

                    if (uwr.result != UnityWebRequest.Result.Success)
                        Debug.LogError(uwr.error);
                    else
                    {
                        // Print Body
                        // Debug.Log(uwr.downloadHandler.text);
                        var responseBody = UploadResponse.Parse(uwr.downloadHandler.text);

                        // Update Bundle information
                        bundle.mTransactionId = responseBody.transactionId;
                        bundle.mUri = String.Format("{0}/{1}", mConfig.gatewayUri, responseBody.transactionId);
                        bundle.mUploadTimestamp = DateTime.Now.ToString();
                        bundle.mNumOfConfirmations = responseBody.status.confirmed.number_of_confirmations;
                        bundle.mVersion = responseBody.version;
                        Debug.Log("Version: " + responseBody.version);

                        bundle.mStatus = "Uploaded";

                        UpdateBundleEntry(bundle);

                        // Make sure this file will get saved properly
                        EditorUtility.SetDirty(bundle);
                    }
                }
            }

            // Save file
            AssetDatabase.SaveAssets();
        }

        public void AddErrorHelpbox(string errorMsg)
        {
            mHelpBoxHolder.Add(new HelpBox(errorMsg, HelpBoxMessageType.Error));
        }

        private void UpdateBundleEntry(ABData bundle)
        {
            if (bundle.mVisualElement != null)
            {
                bundle.mVisualElement.contentContainer.Query<Label>("date-uploaded").First().text = bundle.mUploadTimestamp;
            }
        }
    }
}
