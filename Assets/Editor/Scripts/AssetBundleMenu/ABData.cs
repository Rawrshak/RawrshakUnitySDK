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
        }

        public void FromJSON()
        {
            mHashId = Hash128.Parse(mHash);
        }
    }
}