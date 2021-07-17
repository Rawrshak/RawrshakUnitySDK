using UnityEngine;
using System;
using System.Collections.Generic;

namespace Rawrshak
{
    [Serializable]
    public class AssetBundlesInfo
    {
        [NonSerialized]
        public Dictionary<Hash128, AssetBundleData> mDictionary;

        public List<AssetBundleData> mData;
        
        public static AssetBundlesInfo CreateFromJSON(string jsonString)
        {
            var info = JsonUtility.FromJson<AssetBundlesInfo>(jsonString);

            // Build the dictionary
            info.mDictionary = new Dictionary<Hash128, AssetBundleData>();
            foreach (var bundle in info.mData)
            {
                // Get HashId from Hash String saved in JSON file
                bundle.FromJSON();

                // Add to dictionary
                info.mDictionary.Add(bundle.mHashId, bundle);
            }
            return info;
        }
        
        public string SaveToJSON()
        {
            // Build the mData from the Dictionary
            mData.Clear();
            var iter = mDictionary.GetEnumerator();
            while (iter.MoveNext()) 
            {
                mData.Add(iter.Current.Value);
            }

            return JsonUtility.ToJson(this);
        }
    }


    [Serializable]
    public class AssetBundleData
    {
        [NonSerialized]
        public Hash128 mHashId;
        public string mHash;
        public string mName;

        public bool mSelectedForUploading;

        public string mTransactionId;
        public string mUri;

        public string mUploadedTimestamp;

        public AssetBundleData(Hash128 hash, string name)
        {
            mHashId = hash;
            mHash = hash.ToString();
            mName = name;
            mSelectedForUploading = false;
            mTransactionId = "";
            mUri = "";
            mUploadedTimestamp = DateTime.MinValue.ToString();
        }

        public void FromJSON()
        {
            mHashId = Hash128.Parse(mHash);
        }


        // public string assetBundleLocation;
        // public string transactionId;
        // public bool uploaded;

        // public Assets[] assets;
        // public Dependencies[] dependencies;
        // public Materials[] materials;
    }

    // [Serializable]
    // public class Materials {
    //     public string childMeshName;
    //     public string[] materialNames;
    // }
}