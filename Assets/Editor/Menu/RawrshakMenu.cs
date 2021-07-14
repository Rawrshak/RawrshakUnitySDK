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
            walletManager.Init(settingsManager.mRawrshakSettings, settingsManager.mEthereumSettings, () => { this.Repaint(); });
        }
        settingsManager.mWalletManager = walletManager;

        assetBundleManager = ScriptableObject.CreateInstance<AssetBundleManager>();
        assetBundleManager.Init(settingsManager.mRawrshakSettings);

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
                contentContractManager.LoadUI(rootVisualElement);
                break;
            }
            case "asset-button": {
                assetsManager.LoadUI(rootVisualElement);
                break;
            }
            case "asset-bundle-button": {
                assetBundleManager.LoadUI(rootVisualElement);
                break;
            }
            case "wallet-button":
            default: {
                walletManager.LoadUI(rootVisualElement);
                break;
            }
        }
    }
}