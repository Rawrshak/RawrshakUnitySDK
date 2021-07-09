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
    private static Settings settings;
    private static AssetBundleManager assetBundleManager;

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
        settings = Resources.Load<Settings>("RawrshakSettings");
        if (settings == null) {
            settings = ScriptableObject.CreateInstance<Settings>();
            settings.Init();
            AssetDatabase.CreateAsset(settings, "Assets/Editor/Resources/RawrshakSettings.asset");
            AssetDatabase.SaveAssets();
        }

        assetBundleManager = ScriptableObject.CreateInstance<AssetBundleManager>();
        assetBundleManager.Init(Application.dataPath + "/" + settings.assetBundleFolder);
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
                LoadSettingsPage();
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
            if (password.value == "" || keystoreLocation.value == "") {
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
            if (newPassword.value == "") {
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

    private void LoadSettingsPage() {
        var mainPage = rootVisualElement.Query<Box>("settings-foldout-box").First();

        SerializedObject so = new SerializedObject(settings);
        mainPage.Bind(so);

        var saveButton = rootVisualElement.Query<Button>("save-button").First();
        saveButton.clicked += () => {
            VerifySettings();
            SaveSettings();
        };
    }

    private void LoadContractPage() {
        // Todo
    }

    private void LoadAssetPage() {
        // Todo
    }

    private void LoadAssetBundlePage() {
        var helpboxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();

        var generateAssetBundles = rootVisualElement.Query<Button>("create-asset-bundles-button").First();
        generateAssetBundles.clicked += () => {
            CreateAssetBundles.BuildAllAssetBundles(settings.buildTarget);
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

    private void SaveSettings() {
        var assetBundleFolder = rootVisualElement.Query<TextField>("asset-bundle-folder").First();
        settings.assetBundleFolder = assetBundleFolder.text;

        var buildTarget = rootVisualElement.Query<EnumField>("build-target").First();
        settings.buildTarget = (Rawrshak.SupportedBuildTargets)buildTarget.value;

        var ethereumUri = rootVisualElement.Query<TextField>("ethereum-uri").First();
        settings.ethereumGatewayUri = ethereumUri.text;

        var networkId = rootVisualElement.Query<EnumField>("network-id").First();
        settings.networkId = (Rawrshak.EthereumNetwork)networkId.value;

        var chainId = rootVisualElement.Query<TextField>("chain-id").First();
        settings.chainId = int.Parse(chainId.text);

        var port = rootVisualElement.Query<TextField>("port").First();
        settings.port = int.Parse(port.text);

        var defaultGasPrice = rootVisualElement.Query<TextField>("default-gas-price").First();
        settings.defaultGasPrice = int.Parse(defaultGasPrice.text);

        var graphNodeUri = rootVisualElement.Query<TextField>("graph-node-uri").First();
        settings.graphNodeUri = graphNodeUri.text;

        var arweaveGatewayUri = rootVisualElement.Query<TextField>("arweave-uri").First();
        settings.arweaveGatewayUri = arweaveGatewayUri.text;

        var arweaveWalletFile = rootVisualElement.Query<TextField>("arweave-wallet-file").First();
        settings.arweaveWalletFile = arweaveWalletFile.text;

        AssetDatabase.SaveAssets();
    }

    private void VerifySettings() {
        // Check if asset bundle folder exists
        // Check if we can connect to ethereum 
        // check if we can connect to arweave
        // check if we can connect to the graph node
    }
}