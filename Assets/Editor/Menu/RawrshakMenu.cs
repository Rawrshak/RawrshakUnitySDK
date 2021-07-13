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
    private static WalletManager walletManager; 
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
        // Load Settings
        if (settingsManager == null)
        {
            settingsManager = ScriptableObject.CreateInstance<SettingsManager>();
            settingsManager.Init();
        }

        // Load Wallet
        if (walletManager == null)
        {
            walletManager = ScriptableObject.CreateInstance<WalletManager>();
            walletManager.Init(settingsManager.mRawrshakSettings, () => { this.Repaint(); });
        }

        assetBundleManager = ScriptableObject.CreateInstance<AssetBundleManager>();
        assetBundleManager.Init(Application.dataPath + "/" + settingsManager.mRawrshakSettings.assetBundleFolder);

        contentContractManager = ScriptableObject.CreateInstance<ContentContractManager>();
        contentContractManager.Init(walletManager, settingsManager.mRawrshakSettings);
        
        assetsManager = ScriptableObject.CreateInstance<AssetsManager>();
        assetsManager.Init(walletManager, contentContractManager);
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
                walletManager.LoadUI(rootVisualElement);
                break;
            }
        }
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

}