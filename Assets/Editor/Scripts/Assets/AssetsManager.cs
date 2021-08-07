using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Rawrshak
{
    public class AssetsManager : ScriptableObject
    {
        public Label mWalletLabel;
        public Label mContentContractLabel;
        public Box mAssetListBox;
        public ScrollView mAssetInfoBox;
        public Box mContractInfo;
        public Box mPublicMetadataBox;
        public Box mPrivateMetadataBox;
        public Box mHelpBoxHolder;
        public AssetData mSelected;

        public WalletManager mWallet;
        public ContentContractManager mContentContractManager;

        // Private UXML
        VisualTreeAsset mAssetInfoTreeAsset;
        VisualTreeAsset mPublicMetadataTreeAsset;
        VisualTreeAsset mAssetListEntryTreeAsset;
        VisualTreeAsset mTextMetadataTreeAsset;

        // Static Variables
        public static string ASSETS_FILE_LOCATION = "Assets/Editor/Resources/Assets";

        // Data
        private List<AssetData> mAssetDataList;

        public void Init(WalletManager wallet, ContentContractManager contentContractManager)
        {
            mContentContractManager = contentContractManager;
            mWallet = wallet;
            mSelected = null;

            if (mAssetDataList == null) {
                mAssetDataList = new List<AssetData>();
            }

            // Load all Assets in Resources/Assets
            foreach (AssetData asset in Resources.LoadAll("Assets", typeof(AssetData)))
            {
                // Only Load assets that are stored
                if (EditorUtility.IsPersistent(asset))
                {
                    asset.LoadJson();
                    // Set Non-serialized data to default values
                    mAssetDataList.Add(asset);
                    Debug.Log("Adding Asset: " + asset.name + ", ID: " + asset.GetInstanceID());
                }
            }

            LoadUXML();
        }

        private void LoadUXML()
        {
            mAssetInfoTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetDataInfo.uxml");
            mPublicMetadataTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/PublicAssetMetadata.uxml");
            mAssetListEntryTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetListEntry.uxml");
            mTextMetadataTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/TextMetadata.uxml");
        }

        public void CleanUp()
        {
            // This saves all the content contracts that have not yet been uploaded.
            AssetDatabase.SaveAssets();
        }

        public void LoadUI(VisualElement root)
        {
            mHelpBoxHolder = root.Query<Box>("helpbox-holder").First();
            mAssetListBox = root.Query<Box>("asset-entries").First();
            mAssetInfoBox = root.Query<ScrollView>("asset-info").First();
            mContractInfo = root.Query<Box>("contract-info").First();
            mPublicMetadataBox = root.Query<Box>("public-metadata").First();
            mPrivateMetadataBox = root.Query<Box>("private-metadata").First();
            mWalletLabel = root.Query<Label>("wallet-label").First();
            mContentContractLabel = root.Query<Label>("content-label").First();
            var assetTypeEnumField = root.Query<EnumField>("asset-type").First();
            var assetSubtypeEnumField = root.Query<EnumField>("asset-subtype").First();
            var generateAssetButton = root.Query<Button>("generate-asset-button").First();
            var deployAssetsButton = root.Query<Button>("deploy-button").First();

            // Todo: when type changes, also change the AssetSubtype
            assetTypeEnumField.Init(AssetType.Text);
            assetSubtypeEnumField.Init(AssetSubtype.Text_Custom);

            if (mContentContractManager.mSelected == null || !mContentContractManager.mSelected.isDeployed)
            {
                HelpBox helpbox = new HelpBox("Select a deployed Content Contract in the Content Contract tab.", HelpBoxMessageType.Error);

                mHelpBoxHolder.Add(helpbox);
            }
            else
            {
                mContentContractLabel.text = "Current Content Contract Selected: " + mContentContractManager.mSelected.name;
            }

            if (String.IsNullOrEmpty(mWallet.GetPublicKey()))
            {
                HelpBox helpbox = new HelpBox("Load a wallet in the Wallet tab.", HelpBoxMessageType.Error);
                mHelpBoxHolder.Add(helpbox);
            }
            else
            {
                mWalletLabel.text = "Current Wallet Loaded: " + mWallet.GetPublicKey();
            }

            generateAssetButton.clicked += () => {
                GenerateAsset((AssetType)assetTypeEnumField.value, (AssetSubtype)assetSubtypeEnumField.value);
            };
            
            deployAssetsButton.clicked += () => {
                foreach (var asset in mAssetDataList)
                {
                    if (asset.selectedForUploading)
                    {
                        asset.selectedForUploading = false;
                        asset.deploymentDate = DateTime.Now.ToString();
                        asset.isDeployed = true;

                        // Todo: Delete asset files as it will now be data pulled from
                        //       GraphQL
                    }
                }

                RefreshAssetList();
            };

            RefreshAssetList();
            LoadAssetInfo(mSelected);
        }

        private void GenerateAsset(AssetType type, AssetSubtype subtype)
        {
            Debug.Log("Asset Type: " + type.ToString() + ", Subtype: " + subtype.ToString());
            AssetData asset = ScriptableObject.CreateInstance<AssetData>();
            asset.Init(type, subtype);
            mAssetDataList.Add(asset);
            
            CreateAssetFile(asset);

            LoadAssetInfo(asset);
            Debug.Log("Generating Asset: " + asset.GetInstanceID());

            RefreshAssetList();
        }

        private void LoadAssetInfo(AssetData asset)
        {
            if (asset == null)
            {
                return;
            }
            
            mContractInfo.Clear();
            mPublicMetadataBox.Clear();
            mPrivateMetadataBox.Clear();
            
            TemplateContainer assetTree = mAssetInfoTreeAsset.CloneTree();
            TemplateContainer publicMetadataTree = mPublicMetadataTreeAsset.CloneTree();

            // Add this to info box
            mContractInfo.Add(assetTree);
            mPublicMetadataBox.Add(publicMetadataTree);

            SerializedObject so = new SerializedObject(asset.createData);
            mContractInfo.Bind(so);

            SerializedObject publicMetadataSO = new SerializedObject(asset.publicMetadata);
            mPublicMetadataBox.Bind(publicMetadataSO);
            
            // Load private metadata
            if (asset.privateMetadata != null)
            {
                // Todo: remove this if statement when we have all private data
                TemplateContainer privateMetadataTree = null;
                switch(asset.publicMetadata.subtype)
                {
                    case AssetSubtype.Text_Custom:
                    case AssetSubtype.Text_Lore:
                    case AssetSubtype.Text_Title:
                        privateMetadataTree = mTextMetadataTreeAsset.CloneTree();
                        break;
                    default:
                        break;
                }
                if (privateMetadataTree != null)
                {
                    mPrivateMetadataBox.Add(privateMetadataTree);
                    SerializedObject privateMetadataSO = new SerializedObject(asset.privateMetadata);
                    mPrivateMetadataBox.Bind(privateMetadataSO);
                }
            }
            
            mSelected = asset;
            Debug.Log("Selected Content Contract: " + asset.name);
        }

        private void RefreshAssetList()
        {
            mAssetListBox.Clear();

            foreach (var asset in mAssetDataList)
            {
                TemplateContainer assetTree = mAssetListEntryTreeAsset.CloneTree();
                
                var entry = assetTree.contentContainer.Query<Box>("asset-box").First();
                SerializedObject so = new SerializedObject(asset);
                entry.Bind(so);

                // Set Name label to object
                var nameLabel = assetTree.contentContainer.Query<Label>("name-label").First();
                so = new SerializedObject(asset.publicMetadata);
                nameLabel.Bind(so);

                var deployBox = assetTree.contentContainer.Query<Box>("deploy-box").First();
                var preDeployBox = assetTree.contentContainer.Query<Box>("pre-deploy-box").First();
                var postDeployBox = assetTree.contentContainer.Query<Box>("post-deploy-box").First();

                Debug.Log("Asset Deployed: " + asset.isDeployed);
                if (asset.isDeployed)
                {
                    deployBox.contentContainer.Remove(preDeployBox);
                    
                    var uploadDateLabel = assetTree.contentContainer.Query<Label>("upload-date-label").First();
                    uploadDateLabel.text = asset.deploymentDate;
                }
                else
                {
                    deployBox.contentContainer.Remove(postDeployBox);
                }

                // Select Content Contract Callback
                entry.RegisterCallback<MouseDownEvent>((evt) => {
                    Debug.Log("Info to Display: " + asset.name);
                    LoadAssetInfo(asset);
                });

                mAssetListBox.Add(assetTree);
            }
        }

        private void CreateAssetFile(AssetData asset)
        {
            string fileName = String.Format("{0}/Asset{1}.asset", ASSETS_FILE_LOCATION, asset.GetInstanceID());
            AssetDatabase.CreateAsset(asset, fileName);
            AssetDatabase.SaveAssets();
        }
    }
}