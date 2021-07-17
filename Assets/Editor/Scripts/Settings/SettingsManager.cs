using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Rawrshak;
using System;
using System.IO;
using UnityEditor.Scripting.Python;

namespace Rawrshak
{
    public class SettingsManager : ScriptableObject
    {
        // Settings Files
        public ArweaveSettings mArweaveSettings;
        public EthereumSettings mEthereumSettings;
        public GraphNodeSettings mGraphNodeSettings;
        public RawrshakSettings mRawrshakSettings;

        public WalletManager mWalletManager;

        // UI
        private Box mHelpbox;

        public void Init()
        {
            // Create Settings folder if necessary
            string settingsDirectory = "Assets/Editor/Resources/Settings";
            if(!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }

            // Load Rawrshak Settings (and Initialize if necessary)
            mRawrshakSettings = Resources.Load<RawrshakSettings>("RawrshakSettings");
            if (mRawrshakSettings == null)
            {
                mRawrshakSettings = ScriptableObject.CreateInstance<RawrshakSettings>();
                mRawrshakSettings.Init();
                AssetDatabase.CreateAsset(mRawrshakSettings, "Assets/Editor/Resources/Settings/RawrshakSettings.asset");
            }
            
            // Load Ethereum Settings (and Initialize if necessary)
            mEthereumSettings = Resources.Load<EthereumSettings>("EthereumSettings");
            if (mEthereumSettings == null)
            {
                mEthereumSettings = ScriptableObject.CreateInstance<EthereumSettings>();
                mEthereumSettings.Init();
                AssetDatabase.CreateAsset(mEthereumSettings, "Assets/Editor/Resources/Settings/EthereumSettings.asset");
            }
            
            // Load GraphNode Settings (and Initialize if necessary)
            mGraphNodeSettings = Resources.Load<GraphNodeSettings>("GraphNodeSettings");
            if (mGraphNodeSettings == null)
            {
                mGraphNodeSettings = ScriptableObject.CreateInstance<GraphNodeSettings>();
                mGraphNodeSettings.Init();
                AssetDatabase.CreateAsset(mGraphNodeSettings, "Assets/Editor/Resources/Settings/GraphNodeSettings.asset");
            }
            
            // Load Arweave Settings (and Initialize if necessary)
            mArweaveSettings = Resources.Load<ArweaveSettings>("ArweaveSettings");
            if (mArweaveSettings == null)
            {
                mArweaveSettings = ScriptableObject.CreateInstance<ArweaveSettings>();
                mArweaveSettings.Init();
                AssetDatabase.CreateAsset(mArweaveSettings, "Assets/Editor/Resources/Settings/ArweaveSettings.asset");
            }
            AssetDatabase.SaveAssets();
        }

        public void LoadUI(VisualElement root)
        {
            var developerSettingsFoldout = root.Query<Foldout>("developer-settings").First();
            var rawrshakSettingsFoldout = root.Query<Foldout>("rawrshak-settings").First();
            var ethereumSettingsFoldout = root.Query<Foldout>("ethereum-settings").First();
            var graphnodeSettingsFoldout = root.Query<Foldout>("graphnode-settings").First();
            var arweaveSettingsFoldout = root.Query<Foldout>("arweave-settings").First();
            
            SerializedObject so = new SerializedObject(mRawrshakSettings);
            developerSettingsFoldout.Bind(so);
            rawrshakSettingsFoldout.Bind(so);

            so = new SerializedObject(mEthereumSettings);
            ethereumSettingsFoldout.Bind(so);
            
            so = new SerializedObject(mGraphNodeSettings);
            graphnodeSettingsFoldout.Bind(so);
            
            so = new SerializedObject(mArweaveSettings);
            arweaveSettingsFoldout.Bind(so);

            var verifyButton = root.Query<Button>("verify-button").First();
            verifyButton.clicked += () => {
                VerifySettings();
            };

            mHelpbox = root.Query<Box>("helpbox-holder").First();
        }

        private void VerifySettings() {
            mHelpbox.Clear();

            // Check if asset bundle folder exists
            string assetBundleDirectory = "Assets/" + mRawrshakSettings.assetBundleFolder;
            if(!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            // // Check if we can connect to ethereum 
            // if (mWalletManager)
            // {
            //     CheckEthereumConnection();
            // }

            // check if we can connect to arweave
            if (mArweaveSettings)
            {
                PythonRunner.RunFile($"{Application.dataPath}/Editor/Python/VerifyArweaveConnection.py");
            }

            // check if we can connect to the graph node
        }

        private async void CheckEthereumConnection()
        {
            if (mWalletManager.mWeb3 == null)
            {
                AddErrorHelpbox("Error: Web3 Connection has not been established.");
                return;
            }

            try
            {
                await mWalletManager.mWeb3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError("Web3 Error: " + ex.Message);
                AddErrorHelpbox(ex.Message);
                return;
            }
            mHelpbox.Add(new HelpBox("Verified Ethereum Connection", HelpBoxMessageType.Info));
        }

        public void AddErrorHelpbox(string errorMsg)
        {
            mHelpbox.Add(new HelpBox(errorMsg, HelpBoxMessageType.Error));
        }
    }
}