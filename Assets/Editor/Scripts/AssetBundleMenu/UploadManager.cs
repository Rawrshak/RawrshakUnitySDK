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

            foreach(var bundle in bundles)
            {
                Debug.Log("Uploading bundle: " + bundle.mName);
                EditorCoroutineUtility.StartCoroutine(UploadAssetBundle(bundle), this);
            }

            /******/
            /* The following is specific to Python Gateway upload
            /******/

            // foreach(var bundle in bundles)
            // {
            //     bundleForUpload = bundle;
            //     Debug.Log("Uploading bundle: " + bundleForUpload.mName);
                
            //     // This has to be synchronous because the python script reads 
            //     // off of the bundle listed here. You might want to just have
            //     // the python script load the entire bundle list and upload instead
            //     // of doing it one by one. At which point, you can rever this back
            //     // to a coroutine
            //     // Todo: Update script to upload the entire list of bundle instead
            //     // of doing this one by one. 
            //     UploadAssetBundle();
            //     // EditorCoroutineUtility.StartCoroutine(UploadAssetBundle(), this);
            // }
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

        IEnumerator UploadAssetBundle(ABData bundle)
        {
            string filePath = bundle.mName;
            if (!String.IsNullOrEmpty(mConfig.dwsFolderPath))
            {
                filePath = string.Format("{0}/{1}", mConfig.dwsFolderPath, filePath);
            }
            string uri = String.Format("{0}/{1}/{2}", uploadBaseUrl, mConfig.dwsBucketName, filePath);

            Debug.Log("Uri: " + uri);

            // // Upload the file
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
                    var responseBody = UploadResponse.Parse(uwr.downloadHandler.text);
                    // Debug.Log(uwr.downloadHandler.text);

                    bundle.mTransactionId = responseBody.transactionId;

                    // Todo: replace this with the Arweave.Net gateway
                    bundle.mUri = String.Format("http://159.203.158.0/{0}", responseBody.transactionId);
                    // bundle.mUri = String.Format("https://arweave.net/{0}", responseBody.transactionId);

                    // Save upload time
                    bundle.mUploadTimestamp = DateTime.Now.ToString();
                    bundle.mNumOfConfirmations = responseBody.status.confirmed.number_of_confirmations;
                    Debug.Log("Version: " + responseBody.version);

                    // Todo: Move Asset Bundle to "Uploaded Section"
                    // Todo: Save asset bundle file
                }
            }
        }

        // // Todo: revert this back to a coroutine
        // void UploadAssetBundle()
        // {
        //     if (String.IsNullOrEmpty(mConfig.walletAddress))
        //     {
        //         AddErrorHelpbox("No Wallet Loaded.");
        //         // yield break;
        //     }
        //     PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/UploadAssetBundle.py");
        //     // yield return null;

        //     // Save the info that the python scripts updated
        //     EditorUtility.SetDirty(bundleForUpload);
        //     AssetDatabase.SaveAssets();
        // }

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
