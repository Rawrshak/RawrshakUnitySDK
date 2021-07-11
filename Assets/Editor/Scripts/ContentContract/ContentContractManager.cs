using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[Serializable]
public class ContentContractManager : ScriptableObject
{
    [NonSerialized]
    public Settings mSettings;
    [NonSerialized]
    public Wallet mWallet;
    [NonSerialized]
    public Box mContractEntriesBox;
    [NonSerialized]
    public Box mContentContractInfoBox;
    [NonSerialized]
    public Label mWalletLabel;
    [NonSerialized]
    public Button mGenerateContractButon;

    // Data
    public List<ContentContract> mContentContracts;

    public void Init(Wallet wallet, Settings settings)
    {
        mSettings = settings;
        mWallet = wallet;

        if (mContentContracts == null) {
            mContentContracts = new List<ContentContract>();
        }

        // Load all Content Contracts in Resources/ContentContracts
        foreach (ContentContract contract in Resources.FindObjectsOfTypeAll(typeof(ContentContract)) as ContentContract[])
        {
            if (EditorUtility.IsPersistent(contract))
            {
                mContentContracts.Add(contract);
                Debug.Log("Adding Content Contract: " + contract.mName + ", ID: " + contract.GetInstanceID());
            }
        }
    }

    public void CleanUp()
    {        
        // string data = this.SaveToJSON();
        // Debug.Log("Saving Unpublished Content Contracts.");
        
        // // Delete file and .meta filefirst
        // File.Delete("Assets/Editor/Resources/ContentContractInfo.json");
        // File.Delete("Assets/Editor/Resources/ContentContractInfo.json.meta");
        
        // // write file 
        // StreamWriter writer = new StreamWriter("Assets/Editor/Resources/ContentContractInfo.json", true);
        // writer.WriteLine(data);
        // writer.Close();
        // AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    public void LoadContentContractUI()
    {
        mWalletLabel.text = "Current Wallet Loaded: " + mWallet.publicKey;

        mGenerateContractButon.clicked += () => {
            GenerateContract();
        };

        RefreshContractList();
    }

    private void GenerateContract()
    {
        ContentContract contract = ScriptableObject.CreateInstance<ContentContract>();
        contract.Init(mSettings.developerName, mWallet.publicKey);
        mContentContracts.Add(contract);
        
        CreateContentContractFile(contract);

        LoadContractInfo(contract);
        Debug.Log("Generating Contract from " + contract.mDeveloperAddress);

        RefreshContractList();
    }

    private void LoadContractInfo(ContentContract contract)
    {
        if (mContentContractInfoBox == null)
        {
            // Todo: Throw error or update helpbox
            return;
        }

        mContentContractInfoBox.Clear();
        
        Debug.Log("Generating Contract Info ");
        var content = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/ContentContractInfo.uxml");
        TemplateContainer contentTree = content.CloneTree();

        var infoSection = contentTree.contentContainer.Query<Box>("content-contract-info-section").First();
        
        SerializedObject so = new SerializedObject(contract);
        infoSection.Bind(so);
        
        mContentContractInfoBox.Add(contentTree);
    }

    private void RefreshContractList()
    {
        mContractEntriesBox.Clear();
        var content = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/ContentContractEntry.uxml");
        foreach (var contract in mContentContracts)
        {
            TemplateContainer contentTree = content.CloneTree();
            
            var entry = contentTree.contentContainer.Query<Box>("content-contract-box").First();
            SerializedObject so = new SerializedObject(contract);
            entry.Bind(so);

            var deployBox = contentTree.contentContainer.Query<Box>("deploy-box").First();
            var preDeployBox = contentTree.contentContainer.Query<Box>("pre-deploy-box").First();
            var postDeployBox = contentTree.contentContainer.Query<Box>("post-deploy-box").First();
            if (contract.isDeployed)
            {
                deployBox.contentContainer.Remove(preDeployBox);
                
                var uploadDateLabel = contentTree.contentContainer.Query<Label>("upload-date-label").First();
                uploadDateLabel.text = contract.mContractDeploymentDate;
            }
            else
            {
                deployBox.contentContainer.Remove(postDeployBox);
                var deployButton = contentTree.contentContainer.Query<Button>("deploy-button").First();
                var deleteButton = contentTree.contentContainer.Query<Button>("delete-button").First();

                deployButton.clicked += () => {
                    // Debug.Log("Contract Name: " + contract.mName);
                    contract.isDeployed = true;

                    contract.mContractDeploymentDate = DateTime.Now.ToString();
                    RefreshContractList();
                };

                deleteButton.clicked += () => {
                    Debug.Log("Delete Name: " + contract.name);

                    // Delete file and .meta filefirst
                    string fileName = String.Format("Assets/Editor/Resources/ContentContracts/{0}.asset", contract.name);
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
                // assetBundleData.selected = (evt.target as Label).value;
                Debug.Log("Info to Display: " + contract.mName);
                LoadContractInfo(contract);
            });

            mContractEntriesBox.Add(contentTree);
        }
    }

    private void CreateContentContractFile(ContentContract contract)
    {
        string fileName = String.Format("Assets/Editor/Resources/ContentContracts/Contract_{0}.asset", contract.GetInstanceID());
        AssetDatabase.CreateAsset(contract, fileName);
        AssetDatabase.SaveAssets();
    }
}
