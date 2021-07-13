using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class AssetsManager : ScriptableObject
{
    public Label mWalletLabel;
    public Label mContentContractLabel;
    public Box mAssetListBox;
    public Box mAssetInfoBox;
    public Box mHelpBoxHolder;
    public Button mGenerateAssetButton;
    public Button mDeployAssetsButton;
    public AssetData mSelected;

    public WalletManager mWallet;
    public ContentContractManager mContentContractManager;

    // Private UXML
    VisualTreeAsset mAssetInfoTreeAsset;
    VisualTreeAsset mAssetListEntryTreeAsset;

    // Static Variables
    static string sAssetsFileLocation = "Assets/Editor/Resources/Assets";

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
        foreach (AssetData asset in Resources.FindObjectsOfTypeAll(typeof(AssetData)) as AssetData[])
        {
            // Only Load assets that are stored
            if (EditorUtility.IsPersistent(asset))
            {
                // Set Non-serialized data to default values
                mAssetDataList.Add(asset);
                Debug.Log("Adding Asset: " + asset.mName + ", ID: " + asset.GetInstanceID());
            }
        }

        LoadUXML();
    }

    private void LoadUXML()
    {
        mAssetInfoTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetDataInfo.uxml");
        mAssetListEntryTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetListEntry.uxml");
    }

    public void CleanUp()
    {
        // This saves all the content contracts that have not yet been uploaded.
        AssetDatabase.SaveAssets();
    }

    public void LoadAssetsUI()
    {
        mHelpBoxHolder.Clear();

        if (mContentContractManager.mSelected == null || !mContentContractManager.mSelected.isDeployed)
        {
            HelpBox helpbox = new HelpBox("Select a deployed Content Contract in the Content Contract tab.", HelpBoxMessageType.Error);

            mHelpBoxHolder.Add(helpbox);
        }
        else
        {
            mContentContractLabel.text = "Current Content Contract Selected: " + mContentContractManager.mSelected.mName;
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

        mGenerateAssetButton.clicked += () => {
            GenerateAsset();
        };
        
        mDeployAssetsButton.clicked += () => {
            foreach (var asset in mAssetDataList)
            {
                if (asset.mSelectedForUploading)
                {
                    asset.mSelectedForUploading = false;
                    asset.mAssetDeploymentDate = DateTime.Now.ToString();
                    asset.mIsDeployed = true;

                    // Todo: Delete asset files as it will now be data pulled from
                    //       GraphQL
                }
            }

            RefreshAssetList();
        };

        RefreshAssetList();
        LoadAssetInfo(mSelected);
    }

    private void GenerateAsset()
    {
        AssetData asset = ScriptableObject.CreateInstance<AssetData>();
        asset.Init();
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
        
        mAssetInfoBox.Clear();
        
        TemplateContainer assetTree = mAssetInfoTreeAsset.CloneTree();

        var infoSection = assetTree.contentContainer.Query<Box>("asset-info-section").First();
        SerializedObject so = new SerializedObject(asset);
        infoSection.Bind(so);
        
        // Add this to info box
        mAssetInfoBox.Add(assetTree);

        mSelected = asset;
        Debug.Log("Selected Content Contract: " + asset.mName);
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

            var deployBox = assetTree.contentContainer.Query<Box>("deploy-box").First();
            var preDeployBox = assetTree.contentContainer.Query<Box>("pre-deploy-box").First();
            var postDeployBox = assetTree.contentContainer.Query<Box>("post-deploy-box").First();

            Debug.Log("Asset Deployed: " + asset.mIsDeployed);
            if (asset.mIsDeployed)
            {
                deployBox.contentContainer.Remove(preDeployBox);
                
                var uploadDateLabel = assetTree.contentContainer.Query<Label>("upload-date-label").First();
                uploadDateLabel.text = asset.mAssetDeploymentDate;
            }
            else
            {
                deployBox.contentContainer.Remove(postDeployBox);
            }

            // Select Content Contract Callback
            entry.RegisterCallback<MouseDownEvent>((evt) => {
                Debug.Log("Info to Display: " + asset.mName);
                LoadAssetInfo(asset);
            });

            mAssetListBox.Add(assetTree);
        }
    }

    private void CreateAssetFile(AssetData asset)
    {
        string fileName = String.Format("{0}/Contract_{1}.asset", sAssetsFileLocation, asset.GetInstanceID());
        AssetDatabase.CreateAsset(asset, fileName);
        AssetDatabase.SaveAssets();
    }
}
