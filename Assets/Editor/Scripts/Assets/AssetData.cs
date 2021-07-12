
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
    public string mAssetDeploymentDate;
    public bool mIsDeployed;
    public bool mSelectedForUploading;
    public List<ContractRoyalties> mContractRoyalties;

    public void Init()
    {
        mName = "Asset Name";
        mId = 0; // Todo: randomly generate this id
        mMetadataUri = "...";
        mHiddenMetadataUri = "...";
        mSelectedForUploading = false;
        mIsDeployed = false;
        mAssetDeploymentDate = "...";
    }
}

[Serializable]
public class AssetRoyalties 
{
    public string mAddress;
    public float mRoyalty;
}
