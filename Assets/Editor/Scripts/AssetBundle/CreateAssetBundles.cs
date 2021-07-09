using UnityEditor;
using System.IO;
using Rawrshak;
using UnityEngine;

public class CreateAssetBundles
{
    public static void BuildAllAssetBundles(SupportedBuildTargets buildTarget)
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, 
                                        BuildAssetBundleOptions.None, 
                                        ConvertToBuildTarget(buildTarget));
    }

    static BuildTarget ConvertToBuildTarget(SupportedBuildTargets buildTarget)
    {
        switch (buildTarget) {
            case SupportedBuildTargets.StandaloneWindows64:
                return BuildTarget.StandaloneWindows64;
            case SupportedBuildTargets.Android:
                return BuildTarget.Android;
            case SupportedBuildTargets.WebGL:
                return BuildTarget.WebGL;
            case SupportedBuildTargets.iOS:
                return BuildTarget.iOS;
            case SupportedBuildTargets.StandaloneWindows:
            default:
                return BuildTarget.StandaloneWindows;
        }
    }
}