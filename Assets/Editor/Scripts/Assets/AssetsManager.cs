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
    public AssetData mSelected;

    public Wallet mWallet;
    public ContentContractManager mContentContractManager;

    // Private UXML
    VisualTreeAsset mAssetInfoTreeAsset;
    VisualTreeAsset mAssetListEntryTreeAsset;

    // Static Variables
    // static string sAssetsFileLocation = "Assets/Editor/Resources/Assets/";

    // Data
    // private List<AssetData> mAssetData;

    public void Init(Wallet wallet, ContentContractManager contentContractManager)
    {
        mContentContractManager = contentContractManager;
        mWallet = wallet;
        mSelected = null;

        // if (mAssetData == null) {
        //     mAssetData = new List<AssetData>();
        // }

        // // Load all Assets in Resources/Assets
        // foreach (AssetDAta asset in Resources.FindObjectsOfTypeAll(typeof(AssetDAta)) as AssetDAta[])
        // {
        //     // Only Load assets that are stored
        //     if (EditorUtility.IsPersistent(asset))
        //     {
        //         mAssetData.Add(asset);
        //         Debug.Log("Adding Asset: " + asset.mName + ", ID: " + asset.GetInstanceID());
        //     }
        // }

        LoadUXML();
    }

    private void LoadUXML()
    {
        // mAssetInfoTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetDataInfo.uxml");
        // mAssetListEntryTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetListEntry.uxml");
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

        if (String.IsNullOrEmpty(mWallet.publicKey))
        {
            HelpBox helpbox = new HelpBox("Load a wallet in the Wallet tab.", HelpBoxMessageType.Error);
            mHelpBoxHolder.Add(helpbox);
        }
        else
        {
            mWalletLabel.text = "Current Wallet Loaded: " + mWallet.publicKey;
        }

        mGenerateAssetButton.clicked += () => {
            GenerateAsset();
        };

        RefreshAssetList();
        LoadAssetInfo(mSelected);
    }

    private void GenerateAsset()
    {
        // AssetData asset = ScriptableObject.CreateInstance<AssetData>();
        // asset.Init(mSettings.developerName, mWallet.publicKey);
        // mAssetData.Add(asset);
        
        // CreateAssetFile(asset);

        // LoadAssetInfo(asset);
        // Debug.Log("Generating Contract from " + contract.mDeveloperAddress);

        RefreshAssetList();
    }

    private void LoadAssetInfo(AssetData asset)
    {
        if (asset == null)
        {
            return;
        }
        
        // mAssetInfoBox.Clear();
        
        // TemplateContainer assetTree = mAssetInfoTreeAsset.CloneTree();

        // var infoSection = assetTree.contentContainer.Query<Box>("content-contract-info-section").First();
        // SerializedObject so = new SerializedObject(contract);
        // infoSection.Bind(so);
        
        // // Add this to info box
        // mAssetInfoBox.Add(assetTree);

        mSelected = asset;
        Debug.Log("Selected Content Contract: " + asset.mName);
    }

    private void RefreshAssetList()
    {
        // mContractEntriesBox.Clear();

        // foreach (var contract in mContentContracts)
        // {
        //     TemplateContainer assetTree = mContractListEntry.CloneTree();
            
        //     var entry = assetTree.contentContainer.Query<Box>("content-contract-box").First();
        //     SerializedObject so = new SerializedObject(contract);
        //     entry.Bind(so);

        //     var deployBox = assetTree.contentContainer.Query<Box>("deploy-box").First();
        //     var preDeployBox = assetTree.contentContainer.Query<Box>("pre-deploy-box").First();
        //     var postDeployBox = assetTree.contentContainer.Query<Box>("post-deploy-box").First();
        //     if (contract.isDeployed)
        //     {
        //         deployBox.contentContainer.Remove(preDeployBox);
                
        //         var uploadDateLabel = assetTree.contentContainer.Query<Label>("upload-date-label").First();
        //         uploadDateLabel.text = contract.mContractDeploymentDate;
        //     }
        //     else
        //     {
        //         deployBox.contentContainer.Remove(postDeployBox);
        //         var deployButton = assetTree.contentContainer.Query<Button>("deploy-button").First();
        //         var deleteButton = assetTree.contentContainer.Query<Button>("delete-button").First();

        //         deployButton.clicked += () => {
        //             // Debug.Log("Contract Name: " + contract.mName);
        //             contract.isDeployed = true;

        //             contract.mContractDeploymentDate = DateTime.Now.ToString();
        //             RefreshContractList();
        //         };

        //         deleteButton.clicked += () => {
        //             Debug.Log("Delete Name: " + contract.name);

        //             // Delete file and .meta filefirst
        //             string fileName = String.Format("{0}/{1}.asset", sContentContractFileLocation, contract.name);
        //             File.Delete(fileName);
        //             File.Delete(fileName + ".meta");
        //             AssetDatabase.Refresh();

        //             mContentContracts.Remove(contract);

        //             Debug.Log("Deleting File: " + fileName);
        //             Debug.Log("Content contract list length: " + mContentContracts.Count);
        //             RefreshContractList();
        //         };
        //     }

        //     // Select Content Contract Callback
        //     entry.RegisterCallback<MouseDownEvent>((evt) => {
        //         Debug.Log("Info to Display: " + contract.mName);
        //         LoadContractInfo(contract);
        //     });

        //     mContractEntriesBox.Add(assetTree);
        // }
    }

    // private void CreateContentContractFile(ContentContract contract)
    // {
    //     string fileName = String.Format("{0}/Contract_{1}.asset", sContentContractFileLocation, contract.GetInstanceID());
    //     AssetDatabase.CreateAsset(contract, fileName);
    //     AssetDatabase.SaveAssets();
    // }
}
