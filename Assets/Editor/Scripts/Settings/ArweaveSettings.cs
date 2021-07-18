using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Scripting.Python;
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

        public void Init()
        {
            Debug.Log("Initializing ArweaveSettings.");
            arweaveGatewayUri = "http://arweave.net";
            arweaveWalletFile = "/Asset/WalletFile";
            wallet = "";
            walletBalance = "0.0";
        }
        
        public void LoadWallet()
        {
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/VerifyArweaveConnection.py");
        }
        
        public void RefreshBalance()
        {
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/RefreshWalletBalance.py");
        }
    }
}
