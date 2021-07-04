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

    [MenuItem("Rawrshak/Rawrshak Menu")]
    public static void ShowExample()
    {
        RawrshakMenu wnd = GetWindow<RawrshakMenu>();
        wnd.titleContent = new GUIContent("RawrshakMenu");
    }

    public void OnEnable() {
        InitData();

        LoadUXML();
        LoadUSS();

        LoadToolbar();
        LoadContent(selectedButton);
    }

    private void InitData() {
        wallet = ScriptableObject.CreateInstance<Wallet>();
        wallet.AddOnWalletLoadListner(OnWalletLoad);
        wallet.AddOnWalletLoadErrorListner(OnWalletLoadError);
        
        settings = ScriptableObject.CreateInstance<Settings>();
        settings.load();
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

    private void OnClick(string buttonName) {
        var tabs = rootVisualElement.Query<ToolbarButton>().ToList();
        foreach (ToolbarButton tab in tabs) {
            tab.style.backgroundColor = unselectedBGColor;
        }

        // Update which button is selected
        var button = rootVisualElement.Query<ToolbarButton>(buttonName).First();
        button.style.backgroundColor = selectedBGColor;
        selectedButton = buttonName;

        LoadContent(selectedButton);

        Debug.Log("Selected Button: " + buttonName);
    }

    private void LoadToolbar() {
        // Add callback to the tabs
        var tabs = rootVisualElement.Query<ToolbarButton>().ToList();
        foreach (ToolbarButton tab in tabs) {
            tab.clicked += () => {
                OnClick(tab.name);
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

        // Reset public key
        publicKeyLabel.text = String.Format("Wallet Address: {0}", wallet.getPublicKey());

        // Set up Load Wallet Button
        loadWalletButton.clicked += () => {
            var privateKey = privateKeyTextField.value;
            privateKeyTextField.value = "";
            wallet.LoadWalletFromPrivateKey(privateKey);
        };

        qrCode.image = wallet.qrCodeTexture;

        walletConnectButton.clicked += async () => {
            Debug.Log("Waiting for WalletConnect...");
            await wallet.LoadWalletFromWalletConnect();
            Debug.Log("Wallet Connected!");
        };
    }

    private void LoadSettingsPage() {
        // Todo
    }

    private void LoadContractPage() {
        // Todo
    }

    private void LoadAssetPage() {
        // Todo
    }

    private void LoadAssetBundlePage() {
        // Todo
    }

    private void OnWalletLoad() {
        // Clear Helpbox
        var helpboxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();
        helpboxHolder.Clear();

        // Update Wallet
        var publicKeyLabel = rootVisualElement.Query<Label>("public-key-label").First();
        publicKeyLabel.text = String.Format("Wallet Address: {0}", wallet.getPublicKey());
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