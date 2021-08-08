using UnityEngine;
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

        public bool mSelectedForUploading;

        public ABData(Hash128 hash, string name, string fileLocation)
        {
            mHashId = hash;
            mHash = hash.ToString();
            mName = name;
            mSelectedForUploading = false;
        }

        public void FromJSON()
        {
            mHashId = Hash128.Parse(mHash);
        }
    }
}