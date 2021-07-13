using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


public class RawrshakMenu : EditorWindow
{
    // Default Settings
    Color selectedBGColor = new Color(90f/255f, 90f/255f, 90f/255f, 1f);
    Color unselectedBGColor = new Color(60f/255f, 60f/255f, 60f/255f, 1f);

    // Menu Properties
    string selectedButton = "wallet-button";
    private static Wallet wallet; 
    private static SettingsManager settingsManager;
    private static AssetBundleManager assetBundleManager;
    private static ContentContractManager contentContractManager;
    private static AssetsManager assetsManager;

    [MenuItem("Rawrshak/Rawrshak Menu")]
    public static void ShowExample()
    {
        RawrshakMenu wnd = GetWindow<RawrshakMenu>();
        wnd.titleContent = new GUIContent("RawrshakMenu");
    }

    public void OnEnable() {
        LoadData();

        LoadUXML();
        LoadUSS();

        LoadToolbar();
        LoadContent(selectedButton);
    }

    public void OnDisable() {
        assetBundleManager.CleanUp();
        contentContractManager.CleanUp();
        AssetDatabase.SaveAssets();
        Debug.Log("Menu Disalbed.");
    }

    private void LoadData() {
        // Load Wallet
        wallet = Resources.Load<Wallet>("Wallet");
        if (wallet == null) {
            wallet = ScriptableObject.CreateInstance<Wallet>();
            wallet.Init(
                "Rawrshak Unity SDK Connection",
                "https://app.warriders.com/favicon.ico",
                "Rawrshak Unity SDK",
                "https://app.warriders.com");
            AssetDatabase.CreateAsset(wallet, "Assets/Editor/Resources/Wallet.asset");
            AssetDatabase.SaveAssets();
        }
        wallet.AddOnWalletLoadListner(OnWalletLoad);
        wallet.AddOnWalletLoadErrorListner(OnWalletLoadError);

        // Load Settings
        if (settingsManager == null)
        {
            settingsManager = ScriptableObject.CreateInstance<SettingsManager>();
            settingsManager.Init();
        }

        assetBundleManager = ScriptableObject.CreateInstance<AssetBundleManager>();
        assetBundleManager.Init(Application.dataPath + "/" + settingsManager.mRawrshakSettings.assetBundleFolder);

        contentContractManager = ScriptableObject.CreateInstance<ContentContractManager>();
        contentContractManager.Init(wallet, settingsManager.mRawrshakSettings);
        
        assetsManager = ScriptableObject.CreateInstance<AssetsManager>();
        assetsManager.Init(wallet, contentContractManager);
    }

    private void LoadUXML() {
        // Do it this way
        // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIToolkit/RawrshakMenu.uxml");
        // VisualElement elements = visualTree.Instantiate();
        // root.Add(elements);

        // Or do it this way:
        var original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/RawrshakMenu.uxml");
        TemplateContainer treeAsset = original.CloneTree();
        rootVisualElement.Add(treeAsset);
    }

    private void LoadUSS() {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/USS/RawrshakMenu.uss");
        rootVisualElement.styleSheets.Add(styleSheet);
    }

    private void OnClickToolbarButton(string buttonName) {
        // Save Scriptable Objects first
        AssetDatabase.SaveAssets();

        var tabs = rootVisualElement.Query<ToolbarButton>().ToList();
        foreach (ToolbarButton tab in tabs) {
            tab.style.backgroundColor = unselectedBGColor;
        }

        // Update which button is selected
        var button = rootVisualElement.Query<ToolbarButton>(buttonName).First();
        button.style.backgroundColor = selectedBGColor;
        selectedButton = buttonName;

        LoadContent(selectedButton);

        // Debug.Log("Selected Button: " + buttonName);
    }

    private void LoadToolbar() {
        // Add callback to the tabs
        var tabs = rootVisualElement.Query<ToolbarButton>().ToList();
        foreach (ToolbarButton tab in tabs) {
            tab.clicked += () => {
                OnClickToolbarButton(tab.name);
            };
            tab.style.backgroundColor = unselectedBGColor;
        }

        // Wallet Tab is selected first
        tabs[0].style.backgroundColor = selectedBGColor;
    }

    private void LoadContent(string buttonName) {
        var mainPage = rootVisualElement.Query<Box>("main-page").First();
        mainPage.Clear();

        string umlToLoad = "Assets/Editor/UXML/WalletContent.uxml";
        switch(buttonName) {
            case "settings-button": {
                umlToLoad = "Assets/Editor/UXML/SettingsContent.uxml";
                break;
            }
            case "contract-button": {
                umlToLoad = "Assets/Editor/UXML/ContractContent.uxml";
                break;
            }
            case "asset-button": {
                umlToLoad = "Assets/Editor/UXML/AssetContent.uxml";
                break;
            }
            case "asset-bundle-button": {
                umlToLoad = "Assets/Editor/UXML/AssetBundleContent.uxml";
                break;
            }
            case "wallet-button":
            default: {
                break;
            }
        }

        var content = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(umlToLoad);
        TemplateContainer contentTree = content.CloneTree();
        mainPage.Add(contentTree);
        
        switch(buttonName) {
            case "settings-button": {
                settingsManager.LoadUI(rootVisualElement);
                break;
            }
            case "contract-button": {
                LoadContractPage();
                break;
            }
            case "asset-button": {
                LoadAssetPage();
                break;
            }
            case "asset-bundle-button": {
                LoadAssetBundlePage();
                break;
            }
            case "wallet-button":
            default: {
                LoadWalletPage();
                break;
            }
        }
    }

    private void LoadWalletPage() {
        var publicKeyLabel = rootVisualElement.Query<Label>("public-key-label").First();
        var loadWalletButton = rootVisualElement.Query<Button>("load-wallet-button").First();
        var privateKeyTextField = rootVisualElement.Query<TextField>("private-key-field").First();
        var qrCode = rootVisualElement.Query<Image>("wallet-connect-qrcode").First();
        var walletConnectButton = rootVisualElement.Query<Button>("connect-wallet-button").First();
        var keystoreLocation = rootVisualElement.Query<TextField>("keystore-location").First();
        var password = rootVisualElement.Query<TextField>("password").First();
        var newPassword = rootVisualElement.Query<TextField>("new-password").First();
        var keystoreLoadButton = rootVisualElement.Query<Button>("load-keystore-wallet-button").First();

        // Reset public key
        publicKeyLabel.text = String.Format("Wallet Address: {0}", wallet.publicKey);

        // Set Password field settings
        password.isPasswordField = true;
        password.maskChar = '*';
        newPassword.isPasswordField = true;
        newPassword.maskChar = '*';
        
        // Load Keystore 
        keystoreLocation.value = wallet.keyStoreLocation;
        keystoreLoadButton.clicked += () => {
            if (String.IsNullOrEmpty(password.value) || String.IsNullOrEmpty(keystoreLocation.value)) {
                Debug.Log("shit is null");
                OnWalletLoadError("Error: Empty Keystore location or password.");
                return;
            }
            var pword = password.value;
            password.value = "";
            if (keystoreLocation.value != wallet.keyStoreLocation) {
                wallet.keyStoreLocation = keystoreLocation.value;
            }
            wallet.LoadWalletFromKeyStore(pword);
        };

        // Set up Load Wallet and Save to file Button
        loadWalletButton.clicked += () => {
            if (String.IsNullOrEmpty(newPassword.value)) {
                OnWalletLoadError("Error: Password cannot be empty.");
                return;
            }
            wallet.LoadWalletFromPrivateKey(privateKeyTextField.value);
            wallet.SaveWalletToFile(newPassword.value);
            privateKeyTextField.value = "";
            newPassword.value = "";
        };

        // Set up Wallet Connect
        qrCode.image = wallet.qrCodeTexture;
        walletConnectButton.clicked += async () => {
            Debug.Log("Waiting for WalletConnect...");
            await wallet.LoadWalletFromWalletConnect();
            Debug.Log("Wallet Connected!");
        };
    }

    private void LoadContractPage() {
        var helpboxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();

        contentContractManager.mContractEntriesBox = rootVisualElement.Query<Box>("contract-entries").First();
        contentContractManager.mContentContractInfoBox = rootVisualElement.Query<Box>("content-contract-info").First();
        contentContractManager.mWalletLabel = rootVisualElement.Query<Label>("wallet-label").First();
        contentContractManager.mGenerateContractButton = rootVisualElement.Query<Button>("generate-content-contract-button").First();

        contentContractManager.LoadContentContractUI();
    }

    private void LoadAssetPage() {
        assetsManager.mHelpBoxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();

        assetsManager.mAssetListBox = rootVisualElement.Query<Box>("asset-entries").First();
        assetsManager.mAssetInfoBox = rootVisualElement.Query<Box>("asset-info").First();
        assetsManager.mWalletLabel = rootVisualElement.Query<Label>("wallet-label").First();
        assetsManager.mContentContractLabel = rootVisualElement.Query<Label>("content-label").First();
        assetsManager.mGenerateAssetButton = rootVisualElement.Query<Button>("generate-asset-button").First();
        assetsManager.mDeployAssetsButton = rootVisualElement.Query<Button>("deploy-button").First();

        assetsManager.LoadAssetsUI();
    }

    private void LoadAssetBundlePage() {
        var helpboxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();

        var generateAssetBundles = rootVisualElement.Query<Button>("create-asset-bundles-button").First();
        generateAssetBundles.clicked += () => {
            CreateAssetBundles.BuildAllAssetBundles(settingsManager.mRawrshakSettings.buildTarget);
        };

        assetBundleManager.assetBundleEntries = rootVisualElement.Query<Box>("asset-bundle-entries").First();
        assetBundleManager.uploadedAsssetBundleEntries = rootVisualElement.Query<Box>("uploaded-asset-bundle-entries").First();
        assetBundleManager.assetBundleInfoBox = rootVisualElement.Query<Box>("asset-bundle-info").First();

        // var printButton = rootVisualElement.Query<Button>("print-button").First();
        // printButton.clicked += () => {
        //     assetBundleManager.Refresh();
        // };
        
        var uploadButton = rootVisualElement.Query<Button>("upload-button").First();
        uploadButton.clicked += () => {
            assetBundleManager.UploadAssetBundles();

            // Update UI
            assetBundleManager.Refresh();
            assetBundleManager.RefreshUploadedAssetBundlesBox();
        };

        // Refresh some UI
        assetBundleManager.Refresh();
        assetBundleManager.RefreshUploadedAssetBundlesBox();
    }

    private void OnWalletLoad() {
        // Clear Helpbox
        var helpboxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();
        helpboxHolder.Clear();

        // Update Wallet
        var publicKeyLabel = rootVisualElement.Query<Label>("public-key-label").First();
        publicKeyLabel.text = String.Format("Wallet Address: {0}", wallet.publicKey);
        Debug.Log("Wallet Loaded!");

        // Refresh UI
        Repaint();
    }

    private void OnWalletLoadError(string e) {
        // Add Error Helpbox
        var helpboxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();
        HelpBox helpbox = new HelpBox(e, HelpBoxMessageType.Error);
        helpboxHolder.Clear();
        helpboxHolder.Add(helpbox);
    }
}