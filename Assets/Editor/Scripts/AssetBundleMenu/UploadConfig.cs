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


        public static UploadConfig CreateInstance()
        {
            var config = ScriptableObject.CreateInstance<UploadConfig>();
            
            config.gatewayUri = "http://arweave.net";
            config.walletFile = String.Empty;
            config.walletAddress = String.Empty;
            config.walletBalance = "0.0";

            return config;
        }
    }
}
