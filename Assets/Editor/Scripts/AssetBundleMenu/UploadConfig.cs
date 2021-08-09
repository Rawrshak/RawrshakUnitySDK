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
    public class UploadConfig : ScriptableObject
    {
        public string gatewayUri;
        public string walletFile;
        public string walletAddress;
        public string walletBalance;
        
        Box mHelpBoxHolder;

        public void Init()
        {
            Debug.Log("Initializing UploadManager.");
            gatewayUri = "http://arweave.net";
            walletFile = String.Empty;
            walletAddress = String.Empty;
            walletBalance = "0.0";
        }
    }
}
