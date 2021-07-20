using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Python;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.IO;
using System.Collections.Generic;

namespace Rawrshak
{
    public class ArweaveSettings : ScriptableObject
    {
        public string arweaveGatewayUri;
        public string arweaveWalletFile;
        public string wallet;
        public string walletBalance;

        public AssetBundleData bundleForUpload;

        public void Init()
        {
            Debug.Log("Initializing ArweaveSettings.");
            arweaveGatewayUri = "http://arweave.net";
            arweaveWalletFile = "/Asset/WalletFile";
            wallet = "";
            walletBalance = "0.0";
        }
        
        // Todo: Create an Arweave Manager to do all of these.
        public void LoadWallet()
        {
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/VerifyArweaveConnection.py");
        }
        
        public void RefreshBalance()
        {
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/RefreshWalletBalance.py");
        }

        public void UploadAssetBundle()
        {
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/UploadAssetBundle.py");
        }

        public void CheckStatus()
        {
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/CheckStatus.py");
        }
    }
}
