using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace Rawrshak
{
    [Serializable]
    public class ABData
    {
        public string mHash;
        public string mName;
        public string mFileLocation;
        public string mStatus;
        public int mNumOfConfirmations;
        public string mTransactionId;
        public string mUri;
        public string mUploadTimestamp;
        public string mUploaderAddress;
        public List<string> mAssets;
        public Int64 mFileSize;

        // Non-Serialized Data
        [NonSerialized]
        public Hash128 mHashId;
        [NonSerialized]
        public TemplateContainer mVisualElement;
        [NonSerialized] 
        public bool mMarkedForDelete;
        [NonSerialized] 
        public bool mSelectedForUploading;

        public ABData(Hash128 hash, string name, string fileLocation)
        {
            mHash = hash.ToString();
            mName = name;
            mFileLocation = fileLocation;
            mStatus = "Untracked";
            mNumOfConfirmations = 0;
            mTransactionId = String.Empty;
            mUri = String.Empty;
            mHashId = hash;
            mVisualElement = null;
            mSelectedForUploading = false;
            mMarkedForDelete = false;
            mUploadTimestamp = String.Empty;
            mUploaderAddress = String.Empty;
            UpdateFileSize();
            UpdateAssetNames();
        }

        public void FromJSON()
        {
            mHashId = Hash128.Parse(mHash);
        }

        public void UpdateAssetNames()
        {
            var assetBundle = AssetBundle.LoadFromFile(mFileLocation);
            if (assetBundle == null)
            {
                Debug.LogError("Asset Bundle Not Loaded: " + mName);
                return;
            }

            mAssets = new List<string>();
            var names = assetBundle.GetAllAssetNames();
            foreach(string name in names)
            {
                mAssets.Add(name);
            }
            assetBundle.Unload(true);
        }

        public void UpdateFileSize()
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(mFileLocation);
            if (fileInfo.Exists)
            {
                mFileSize = fileInfo.Length;
            }
            else
            {
                mFileSize = 0;
            }
        }
    }
}