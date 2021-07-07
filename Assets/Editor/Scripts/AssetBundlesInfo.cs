using UnityEngine;
using System;

[Serializable]
public class AssetBundlesInfo
{
    public AssetBundleData[] uploadedAssetBundles;
    
    public static AssetBundlesInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<AssetBundlesInfo>(jsonString);
    }
}


[Serializable]
public class AssetBundleData
{
    public int hashId;
    public string name;
    public string assetBundleLocation;
    public string transactionId;
    public bool uploaded;

    // public Assets[] assets;
    // public Dependencies[] dependencies;
    // public Materials[] materials;
}

// [Serializable]
// public class Materials {
//     public string childMeshName;
//     public string[] materialNames;
// }