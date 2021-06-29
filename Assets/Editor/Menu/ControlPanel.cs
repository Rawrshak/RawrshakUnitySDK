using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class ControlPanel : EditorWindow
{
    Texture2D headerTexture;

    Color headerBGColor = new Color(13f/255f, 32f/255f, 44f/255f, 1f);

    Rect headerSection;
    Rect tabsSection;
    Rect mainSection;

    GUISkin skin;
    GUIStyle tabStyle;

    private static WalletData walletData; 
    private int selectedTab = 0;
    private int selectedWalletLoad = 0;

    [MenuItem("Rawrshak/Control Panel")]
    static void OpenWindow()
    {
        // javascriptRunner = new JavascriptRunner();
        var window = GetWindow<ControlPanel>();
        window.titleContent = new GUIContent("Rawrshak Control Panel");
        // ControlPanel window = (ControlPanel)GetWindow(typeof(ControlPanel));
        window.minSize = new Vector2(450, 600);
        window.Show();
    }

    // This is similar to OnStart() or OnAwake()
    private void OnEnable() {
        InitTextures();   
        InitData();
        skin = Resources.Load<GUISkin>("GuiStyles/ControlPanel");
        tabStyle = skin.GetStyle("HeaderTab");
    }

    private void InitTextures() {
        headerTexture = new Texture2D(1,1);
        headerTexture.SetPixel(0, 0, headerBGColor);
        headerTexture.Apply();
    }

    private void InitData() {
        walletData = ScriptableObject.CreateInstance<WalletData>();
    }

    // This is similar to OnUpdate(). This is called per UI interaction.
    private void OnGUI() {
        DrawLayouts();
        DrawHeader();
        DrawTabs();
        DrawMain();
    }

    private void DrawLayouts() {
        headerSection.x = 0;
        headerSection.y = 0;
        headerSection.width = position.width;
        headerSection.height = 50;

        GUI.DrawTexture(headerSection, headerTexture);
        
        tabsSection.x = 0;
        tabsSection.y = headerSection.height;
        tabsSection.width = position.width;
        tabsSection.height = 20;

        mainSection.x = 0;
        mainSection.y = tabsSection.y + tabsSection.height;
        mainSection.width = position.width;
        mainSection.height = position.height - tabsSection.height - headerSection.height;

        // GUI.DrawTexture(mainSection, headerTexture);
    }

    private void DrawHeader() {
        GUILayout.BeginArea(headerSection);
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Rawrshak", skin.GetStyle("HeaderText"));
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    private void DrawTabs() {
        GUILayout.BeginArea(tabsSection);

        string[] tabs = {"Wallet", "Settings", "Contracts", "Assets", "Asset Bundles"};                
        selectedTab = GUILayout.Toolbar(selectedTab, tabs);

        GUILayout.EndArea();
    }

    private void DrawMain() {
        
        GUILayout.BeginArea(mainSection);
        switch(selectedTab) {
            case 0: {
                DrawWalletTab();
                break;
            }
            default: {
                break;
            }
        }
        GUILayout.EndArea();
    }
    private void DrawWalletTab() {
        // GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Private Key", GUILayout.Width(position.width/2f), GUILayout.Height(30))) {
            selectedWalletLoad = 0;
        }
        if (GUILayout.Button("WalletConnect", GUILayout.Width(position.width/2f), GUILayout.Height(30))) {
            selectedWalletLoad = 1;
        }

        EditorGUILayout.EndHorizontal();
        
        GUILayout.FlexibleSpace();

        switch(selectedWalletLoad) {
            case 1: {
                break;
            }
            case 0:
            default: {
                // Todo: show private key
                // Todo: show WalletConnect
                walletData.privateKey = EditorGUILayout.TextField("Private Key", walletData.privateKey);
                Debug.Log("Wallet Private Data: " + walletData.privateKey);
                break;
            }
        }

        GUILayout.FlexibleSpace();
    }

        // GUILayout.Label("Rawrshak Control Panel", EditorStyles.boldLabel);
        // metadataFolder = EditorGUILayout.TextField("MetaData Folder", metadataFolder);
        // metadataName = EditorGUILayout.TextField("MetaData Name", metadataName);
}