using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Python;
using System;
using System.Collections.Generic;

namespace Rawrshak
{
    public class ArweaveSettings : ScriptableObject
    {
        public string arweaveGatewayUri;
        public string arweaveWalletFile;
        public string wallet;

        public void Init()
        {
            Debug.Log("Initializing ArweaveSettings.");
            arweaveGatewayUri = "http://arweave.net";
            arweaveWalletFile = "/Asset/WalletFile";
            wallet = "";
        }
        
        public void LoadWallet()
        {
            PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/VerifyArweaveConnection.py");
        }
    }
}
