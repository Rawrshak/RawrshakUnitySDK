using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace Rawrshak
{
    [Serializable]
    public class ABData
    {
        [NonSerialized]
        public Hash128 mHashId;
        public string mHash;
        public string mName;

        [NonSerialized]
        public TemplateContainer mVisualElement;
        [NonSerialized] 
        public bool mMarkedForDelete;

        public bool mSelectedForUploading;

        public ABData(Hash128 hash, string name)
        {
            mHashId = hash;
            mHash = hash.ToString();
            mName = name;
            mSelectedForUploading = false;
            mMarkedForDelete = false;
        }

        public void FromJSON()
        {
            mHashId = Hash128.Parse(mHash);
        }
    }
}