
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Rawrshak;

public class AssetData : ScriptableObject
{
    public string mName;
    public int mId;
    public string mMetadataUri;
    public string mHiddenMetadataUri;
    public List<ContractRoyalties> mContractRoyalties;

    public void Init(string developerName, string developerAddress)
    {
        mName = "Content Contract Name";
        mId = 0; // Todo: randomly generate this id
        mMetadataUri = "...";
        mHiddenMetadataUri = "...";
    }
}

[Serializable]
public class AssetRoyalties 
{
    public string mAddress;
    public float mRoyalty;
}
