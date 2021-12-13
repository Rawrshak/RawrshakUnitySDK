using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace Rawrshak
{
    [Serializable]
    public class AssetBundleData : ScriptableObject
    {
        public string mHash;
        public string mName;
        public string mFileLocation;
        public List<string> mAssets;
        public Int64 mFileSize;
        public SupportedBuildTargets mBuildTarget;
        public SupportedEngine mEngine = SupportedEngine.Unity;
        public string mUnityVersion;
        public int mVersion;

        // Non-Serialized Data
        [NonSerialized]
        public Hash128 mHashId;
        [NonSerialized]
        public TemplateContainer mVisualElement;
        [NonSerialized] 
        public bool mSelected;

        public static AssetBundleData CreateInstance(Hash128 hash, string name, string fileLocation, SupportedBuildTargets buildTarget)
        {
            var data = ScriptableObject.CreateInstance<AssetBundleData>();
            data.Init(hash, name, fileLocation, buildTarget);
            return data;
        }

        private void Init(Hash128 hash, string name, string fileLocation, SupportedBuildTargets buildTarget)
        {
            mHash = hash.ToString();
            mName = name;
            mFileLocation = fileLocation;
            mHashId = hash;
            mVisualElement = null;
            mSelected = false;
            mBuildTarget = buildTarget;
            mUnityVersion = Application.unityVersion;
            mVersion = 0;
            UpdateFileSize();
            UpdateAssetNames();
        }

        public void SetHash128()
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