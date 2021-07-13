using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Rawrshak;
using System.IO;

public class SettingsManager : ScriptableObject
{
    // Settings Files
    public ArweaveSettings mArweaveSettings;
    public EthereumSettings mEthereumSettings;
    public GraphNodeSettings mGraphNodeSettings;
    public RawrshakSettings mRawrshakSettings;

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
        // Check if asset bundle folder exists
        // Check if we can connect to ethereum 
        // check if we can connect to arweave
        // check if we can connect to the graph node
    }
}