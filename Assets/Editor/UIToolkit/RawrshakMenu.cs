using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class RawrshakMenu : EditorWindow
{
    string selectedButton = "wallet-button";
    Color selectedBGColor = new Color(90f/255f, 90f/255f, 90f/255f, 1f);
    Color unselectedBGColor = new Color(60f/255f, 60f/255f, 60f/255f, 1f);

    ToolbarButton walletButton;

    [MenuItem("Rawrshak/Rawrshak Menu")]
    public static void ShowExample()
    {
        RawrshakMenu wnd = GetWindow<RawrshakMenu>();
        wnd.titleContent = new GUIContent("RawrshakMenu");
    }

    public void OnEnable() {
        LoadUXML();
        LoadUSS();

        LoadButtons();
        LoadContent(selectedButton);
    }

    // public void OnDisable() {
    //     UnloadButtons();
    // }

    private void LoadUXML() {
        // Do it this way
        // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIToolkit/RawrshakMenu.uxml");
        // VisualElement elements = visualTree.Instantiate();
        // root.Add(elements);

        // Or do it this way:
        var original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIToolkit/RawrshakMenu.uxml");
        TemplateContainer treeAsset = original.CloneTree();
        rootVisualElement.Add(treeAsset);
    }

    private void LoadUSS() {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/UIToolkit/RawrshakMenu.uss");
        rootVisualElement.styleSheets.Add(styleSheet);
    }

    private void OnClick(string buttonName) {
        var tabs = rootVisualElement.Query<ToolbarButton>().ToList();
        foreach (ToolbarButton tab in tabs) {
            tab.style.backgroundColor = unselectedBGColor;
        }

        var button = rootVisualElement.Query<ToolbarButton>(buttonName).First();
        button.style.backgroundColor = selectedBGColor;

        selectedButton = buttonName;

        LoadContent(buttonName);

        // Debug.Log("Selected Button: " + buttonName);
    }

    private void LoadButtons() {
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

        string umlToLoad = "Assets/Editor/UIToolkit/WalletContent.uxml";
        switch(buttonName) {
            case "settings-button": {
                umlToLoad = "Assets/Editor/UIToolkit/SettingsContent.uxml";
                break;
            }
            case "contract-button": {
                umlToLoad = "Assets/Editor/UIToolkit/ContractContent.uxml";
                break;
            }
            case "asset-button": {
                umlToLoad = "Assets/Editor/UIToolkit/AssetContent.uxml";
                break;
            }
            case "asset-bundle-button": {
                umlToLoad = "Assets/Editor/UIToolkit/AssetBundleContent.uxml";
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
    }

    public void CreateGUI()
    {
        // // Each editor window contains a root VisualElement object
        // VisualElement root = rootVisualElement;

        // // A stylesheet can be added to a VisualElement.
        // // The style will be applied to the VisualElement and all of its children.

        // // // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // // VisualElement label = new Label("Hello World! From C#");
        // // root.Add(label);
        // // VisualElement labelWithStyle = new Label("Hello World! With Style");
        // // labelWithStyle.styleSheets.Add(styleSheet);
        // // root.Add(labelWithStyle);

        // // Import UXML
        // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIToolkit/RawrshakMenu.uxml");
        // VisualElement elements = visualTree.Instantiate();
        // root.Add(elements);

        // // To get an element:
        // Label label = root
    }
}