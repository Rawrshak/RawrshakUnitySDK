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

        // Todo: Remove Wallet file. Request Wallet Address and Wallet Balance API
        public string walletFile;
        public string walletAddress;
        public string walletBalance;

        // DWS
        public string dwsBucketName;
        public string dwsFolderPath;
        public string dwsApiKey;
        
        Box mHelpBoxHolder;


        public static UploadConfig CreateInstance()
        {
            var config = ScriptableObject.CreateInstance<UploadConfig>();
            
            config.gatewayUri = "http://arweave.net";
            config.walletFile = String.Empty;
            config.walletAddress = String.Empty;
            config.walletBalance = "0.0";
            config.dwsBucketName = String.Empty;
            config.dwsFolderPath = String.Empty;
            config.dwsApiKey = String.Empty;

            return config;
        }
    }
}
