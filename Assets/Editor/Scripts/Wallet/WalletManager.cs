using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Rawrshak;
using UnityEngine.Events;
using System;

public class WalletManager : ScriptableObject
{
    // Wallet Properties
    private string mPublicKey;
    private WalletType mWalletType;

    private WalletConnectManager mWalletConnectManager;

    private PrivateWalletManager mPrivateWalletManager;

    private RawrshakSettings mRawrshakSettings;

    // Web3 
    // public Web3 web3;

    // UI
    private Box mHelpbox;
    private Label mPublicKeyLabel;
    private UnityEvent RepaintUI = new UnityEvent();
    
    public void Init(RawrshakSettings rawrshakSettings, UnityAction repaintUI)
    {
        mRawrshakSettings = rawrshakSettings;
        RepaintUI.AddListener(repaintUI);
        mPublicKey = String.Empty;
        mWalletType = WalletType.None;
        
        // Load WalletConnect Manager (and Initialize if necessary)
        mWalletConnectManager = Resources.Load<WalletConnectManager>("WalletConnectManager");
        if (mWalletConnectManager == null)
        {
            mWalletConnectManager = ScriptableObject.CreateInstance<WalletConnectManager>();
            mWalletConnectManager.Init();
            AssetDatabase.CreateAsset(mWalletConnectManager, "Assets/Editor/Resources/WalletConnectManager.asset");
        }

        // Load PrivateWallet (and Initialize if necessary)
        mPrivateWalletManager = Resources.Load<PrivateWalletManager>("PrivateWalletManager");
        if (mPrivateWalletManager == null)
        {
            mPrivateWalletManager = ScriptableObject.CreateInstance<PrivateWalletManager>();
            mPrivateWalletManager.Init();
            AssetDatabase.CreateAsset(mPrivateWalletManager, "Assets/Editor/Resources/PrivateWalletManager.asset");
        }
        AssetDatabase.SaveAssets();
        
        // Add Listener 
        mPrivateWalletManager.SetListeners(OnWalletLoad, OnWalletLoadError);
        mWalletConnectManager.SetListeners(OnWalletLoad, OnWalletLoadError);
    }

    public void LoadUI(VisualElement root)
    {
        mHelpbox = root.Query<Box>("helpbox-holder").First();
        mPublicKeyLabel = root.Query<Label>("public-key-label").First();

        // Reset public key
        mPublicKeyLabel.text = String.Format("Wallet Address: {0}", mPublicKey);

        mPrivateWalletManager.LoadUI(root, mRawrshakSettings.defaultKeystoreLocation);

        // WalletConnect Manager UI
        mWalletConnectManager.LoadUI(root);
    }

    public string GetPublicKey()
    {
        return mPublicKey;
    }
    
    private void OnWalletLoad(WalletType newWalletType)
    {
        // Set Loaded wallet type
        mWalletType = newWalletType;
        if (mWalletType == WalletType.PrivateWallet)
        {
            mPublicKey = mPrivateWalletManager.GetPublicKey();
        }
        else
        {
            mPublicKey = mWalletConnectManager.GetPublicKey();
        }

        // Clear Helpbox
        mHelpbox.Clear();

        // Update Wallet
        mPublicKeyLabel.text = String.Format("Wallet Address: {0}", mPublicKey);
        Debug.Log(mPublicKeyLabel.text);

        // Refresh UI
        RepaintUI.Invoke();
    }

    private void OnWalletLoadError(string errorMsg)
    {
        // Add Error Helpbox
        HelpBox helpbox = new HelpBox(errorMsg, HelpBoxMessageType.Error);
        mHelpbox.Clear();
        mHelpbox.Add(helpbox);
    }
}