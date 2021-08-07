
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Numerics;
using Rawrshak;

namespace Rawrshak
{
    public class AssetData : ScriptableObject
    {
        public string deploymentDate;
        public bool isDeployed;
        public bool selectedForUploading;
        public CreateData createData;
        public PublicAssetMetadata publicMetadata;
        public ScriptableObject privateMetadata;
        public string publicMetadataJson;
        public string privateMetadataJson;

        public void Init(AssetType type, AssetSubtype subtype)
        {
            selectedForUploading = false;
            isDeployed = false;
            deploymentDate = String.Empty;
            createData = ScriptableObject.CreateInstance<CreateData>();

            // Load or create public metadata
            string publicMetadataName = String.Format("Public{0}", this.GetInstanceID());
            this.publicMetadataJson = publicMetadataName;
            publicMetadata = PublicAssetMetadata.CreateFromJSON(String.Format("Assets/{0}.json", publicMetadataName));
            if (publicMetadata == null)
            {
                publicMetadata = ScriptableObject.CreateInstance<PublicAssetMetadata>();
                publicMetadata.Init(type, subtype);
                publicMetadata.SaveToJSON(String.Format("{0}/{1}.json", AssetsManager.ASSETS_FILE_LOCATION, publicMetadataName));
            }
            publicMetadata.fileName = publicMetadataName;
            
            // Load or create private metadata
            string privateMetadataName = String.Format("Private{0}", this.GetInstanceID());
            this.privateMetadataJson = privateMetadataName;
            privateMetadata = TextAssetMetadata.CreateFromJSON(String.Format("Assets/{0}.json", privateMetadataName));
            if (privateMetadata == null)
            {
                privateMetadata = CreatePrivateMetadata(subtype);
                (privateMetadata as TextAssetMetadata).SaveToJSON(String.Format("{0}/{1}.json", AssetsManager.ASSETS_FILE_LOCATION, privateMetadataName));
            }
            (privateMetadata as TextAssetMetadata).fileName = privateMetadataName;
        }

        public void LoadJson()
        {
            publicMetadata = PublicAssetMetadata.CreateFromJSON(String.Format("Assets/{0}", publicMetadataJson));
            privateMetadata = TextAssetMetadata.CreateFromJSON(String.Format("Assets/{0}", privateMetadataJson));
            if (publicMetadata == null) 
            {
                Debug.LogError("Public Metadata is null");
            }
            if (privateMetadata == null) 
            {
                Debug.LogError("private Metadata is null");
            }
        }

        private ScriptableObject CreatePrivateMetadata(AssetSubtype subtype)
        {
            switch (subtype)
            {
                case AssetSubtype.Text_Custom:
                {
                    return ScriptableObject.CreateInstance<CustomTextMetadata>();
                }
                case AssetSubtype.Text_Lore:
                {
                    return ScriptableObject.CreateInstance<LoreMetadata>();
                }
                case AssetSubtype.Text_Title:
                {
                    return ScriptableObject.CreateInstance<TitleMetadata>();
                }
                default:
                {
                    break;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class CreateData : ScriptableObject
    {
        public int tokenId;
        public string publicMetadataUri;
        public string privateMetadataUri;
        public UInt64 maxSupply; // Todo: in eth
        public List<AssetRoyalties> fees;
    }

    [Serializable]
    public class AssetRoyalties 
    {
        public string account;
        public float rate;  // Todo: convert to eth
    }

}