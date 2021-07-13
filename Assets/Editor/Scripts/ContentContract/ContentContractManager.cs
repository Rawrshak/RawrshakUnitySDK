using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ContentContractManager : ScriptableObject
{
    public RawrshakSettings mSettings;
    public WalletManager mWallet;
    public Box mContractEntriesBox;
    public Box mContentContractInfoBox;
    public Label mWalletLabel;
    public Button mGenerateContractButton;
    public ContentContract mSelected;

    // Private UXML
    VisualTreeAsset mContractInfoAsset;
    VisualTreeAsset mContractListEntry;

    // Static Variables
    static string sContentContractFileLocation = "Assets/Editor/Resources/ContentContracts";

    // Data
    private List<ContentContract> mContentContracts;

    public void Init(WalletManager wallet, RawrshakSettings settings)
    {
        mSettings = settings;
        mWallet = wallet;
        mSelected = null;

        if (mContentContracts == null) {
            mContentContracts = new List<ContentContract>();
        }

        // Load all Content Contracts in Resources/ContentContracts
        foreach (ContentContract contract in Resources.FindObjectsOfTypeAll(typeof(ContentContract)) as ContentContract[])
        {
            // Only Load contracts that are stored
            if (EditorUtility.IsPersistent(contract))
            {
                mContentContracts.Add(contract);
                Debug.Log("Adding Content Contract: " + contract.mName + ", ID: " + contract.GetInstanceID());
            }
        }

        LoadUXML();
    }

    private void LoadUXML()
    {
        mContractInfoAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/ContentContractInfo.uxml");
        mContractListEntry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/ContentContractEntry.uxml");
    }

    public void CleanUp()
    {
        // This saves all the content contracts that have not yet been uploaded.
        AssetDatabase.SaveAssets();
    }

    public void LoadContentContractUI()
    {
        mWalletLabel.text = "Current Wallet Loaded: " + mWallet.GetPublicKey();

        mGenerateContractButton.clicked += () => {
            GenerateContract();
        };

        RefreshContractList();
        LoadContractInfo(mSelected);
    }

    private void GenerateContract()
    {
        ContentContract contract = ScriptableObject.CreateInstance<ContentContract>();
        contract.Init(mSettings.developerName, mWallet.GetPublicKey());
        mContentContracts.Add(contract);
        
        CreateContentContractFile(contract);

        LoadContractInfo(contract);
        Debug.Log("Generating Contract from " + contract.mDeveloperAddress);

        RefreshContractList();
    }

    private void LoadContractInfo(ContentContract contract)
    {
        if (contract == null)
        {
            return;
        }
        
        mContentContractInfoBox.Clear();
        
        TemplateContainer assetTree = mContractInfoAsset.CloneTree();

        var infoSection = assetTree.contentContainer.Query<Box>("content-contract-info-section").First();
        SerializedObject so = new SerializedObject(contract);
        infoSection.Bind(so);
        
        // Add this to info box
        mContentContractInfoBox.Add(assetTree);

        mSelected = contract;
        Debug.Log("Selected Content Contract: " + contract.mName);
    }

    private void RefreshContractList()
    {
        mContractEntriesBox.Clear();

        foreach (var contract in mContentContracts)
        {
            TemplateContainer assetTree = mContractListEntry.CloneTree();
            
            var entry = assetTree.contentContainer.Query<Box>("content-contract-box").First();
            SerializedObject so = new SerializedObject(contract);
            entry.Bind(so);

            var deployBox = assetTree.contentContainer.Query<Box>("deploy-box").First();
            var preDeployBox = assetTree.contentContainer.Query<Box>("pre-deploy-box").First();
            var postDeployBox = assetTree.contentContainer.Query<Box>("post-deploy-box").First();
            if (contract.isDeployed)
            {
                deployBox.contentContainer.Remove(preDeployBox);
                
                var uploadDateLabel = assetTree.contentContainer.Query<Label>("upload-date-label").First();
                uploadDateLabel.text = contract.mContractDeploymentDate;
            }
            else
            {
                deployBox.contentContainer.Remove(postDeployBox);
                var deployButton = assetTree.contentContainer.Query<Button>("deploy-button").First();
                var deleteButton = assetTree.contentContainer.Query<Button>("delete-button").First();

                deployButton.clicked += () => {
                    // Debug.Log("Contract Name: " + contract.mName);
                    contract.isDeployed = true;

                    contract.mContractDeploymentDate = DateTime.Now.ToString();
                    RefreshContractList();
                };

                deleteButton.clicked += () => {
                    Debug.Log("Delete Name: " + contract.name);

                    // Delete file and .meta filefirst
                    string fileName = String.Format("{0}/{1}.asset", sContentContractFileLocation, contract.name);
                    File.Delete(fileName);
                    File.Delete(fileName + ".meta");
                    AssetDatabase.Refresh();

                    mContentContracts.Remove(contract);

                    Debug.Log("Deleting File: " + fileName);
                    Debug.Log("Content contract list length: " + mContentContracts.Count);
                    RefreshContractList();
                };
            }

            // Select Content Contract Callback
            entry.RegisterCallback<MouseDownEvent>((evt) => {
                Debug.Log("Info to Display: " + contract.mName);
                LoadContractInfo(contract);
            });

            mContractEntriesBox.Add(assetTree);
        }
    }

    private void CreateContentContractFile(ContentContract contract)
    {
        string fileName = String.Format("{0}/Contract_{1}.asset", sContentContractFileLocation, contract.GetInstanceID());
        AssetDatabase.CreateAsset(contract, fileName);
        AssetDatabase.SaveAssets();
    }
}
