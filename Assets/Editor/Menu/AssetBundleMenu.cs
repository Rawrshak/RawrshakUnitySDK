using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rawrshak
{
    public class AssetBundleMenu : EditorWindow
    {
        // Private Menu Properties
        AssetBundleMenuConfig mConfig;
        ABManager mAssetBundleManager;
        ABViewer mViewer;
        public static UploadManager mUploadManager;

        // UI
        Box mHelpBoxHolder;

        // Static Properties
        public static string ASSET_BUNDLES_FOLDER = "AssetBundles";
        public static string RESOURCES_FOLDER = "Assets/Editor/Resources";
        public static string ASSET_BUNDLES_MENU_CONFIG_DIRECTORY = "AssetBundlesMenuConfig";
        static string ASSET_BUNDLES_MENU_CONFIG_FILE = "MenuConfig";
        static Color selectedBGColor = new Color(90f/255f, 90f/255f, 90f/255f, 1f);
        static Color unselectedBGColor = new Color(60f/255f, 60f/255f, 60f/255f, 1f);


        [MenuItem("Rawrshak/Asset Bundles")]
        public static void ShowExample()
        {
            AssetBundleMenu wnd = GetWindow<AssetBundleMenu>();
            wnd.titleContent = new GUIContent("Asset Bundles");
            wnd.minSize = new Vector2(1200, 400);
        }

        public void OnEnable() {
            LoadData();

            LoadUXML();
            LoadUSS();

            LoadUI();
            Debug.Log("AssetBundleMenu Enabled.");
        }

        public void OnDisable()
        {
            AssetDatabase.SaveAssets();

            ScriptableObject.DestroyImmediate(mAssetBundleManager);
            ScriptableObject.DestroyImmediate(mViewer);
            ScriptableObject.DestroyImmediate(mUploadManager);

            Debug.Log("AssetBundleMenu Disabled.");
        }

        private void LoadData() {
            // Create Settings folder if necessary
            if(!Directory.Exists(String.Format("{0}/{1}", RESOURCES_FOLDER, ASSET_BUNDLES_MENU_CONFIG_DIRECTORY)))
            {
                Directory.CreateDirectory(String.Format("{0}/{1}", RESOURCES_FOLDER, ASSET_BUNDLES_MENU_CONFIG_DIRECTORY));
            }

            // Load Rawrshak Settings (and Initialize if necessary)
            mConfig = Resources.Load<AssetBundleMenuConfig>(String.Format("{0}/{1}", ASSET_BUNDLES_MENU_CONFIG_DIRECTORY, ASSET_BUNDLES_MENU_CONFIG_FILE));
            if (mConfig == null)
            {
                mConfig = ScriptableObject.CreateInstance<AssetBundleMenuConfig>();
                mConfig.Init();
                AssetDatabase.CreateAsset(mConfig, String.Format("{0}/{1}/{2}.asset", RESOURCES_FOLDER, ASSET_BUNDLES_MENU_CONFIG_DIRECTORY, ASSET_BUNDLES_MENU_CONFIG_FILE));
            }

            if (mAssetBundleManager == null)
            {
                mAssetBundleManager = ScriptableObject.CreateInstance<ABManager>();
                mAssetBundleManager.Init(mConfig.assetBundleFolder, mConfig.buildTarget.ToString());
            }

            if (mUploadManager == null)
            {
                mUploadManager = ScriptableObject.CreateInstance<UploadManager>();
            }

            if (mViewer == null)
            {
                mViewer = ScriptableObject.CreateInstance<ABViewer>();
            }

            // Set BundleSelected callback
            mAssetBundleManager.SetBundleSelectedCallback(mViewer.SetAssetBundle);
            mAssetBundleManager.SetUploadBundleCallback(mUploadManager.UploadBundles);
            mViewer.SetCheckStatusCallback(mUploadManager.CheckUploadStatus);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void LoadUXML() {
            var original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML/AssetBundleMenu/AssetBundleMenu.uxml");
            TemplateContainer treeAsset = original.CloneTree();
            rootVisualElement.Add(treeAsset);
        }

        private void LoadUSS() {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/USS/AssetBundleMenu/AssetBundleMenu.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void LoadUI() {
            // Build Target enum
            var settingsFoldout = rootVisualElement.Query<Foldout>("settings").First();
            var buildTargetEnumField = rootVisualElement.Query<EnumField>("build-target").First();

            SerializedObject so = new SerializedObject(mConfig);
            settingsFoldout.Bind(so);

            buildTargetEnumField.Init(mConfig.buildTarget);
            buildTargetEnumField.RegisterCallback<ChangeEvent<System.Enum>>((evt) => {
                // // Update the Build Target
                var newTarget = (Rawrshak.SupportedBuildTargets)evt.newValue;
                var newDirectory = String.Format("{0}/{1}", ASSET_BUNDLES_FOLDER, newTarget.ToString());
                
                // Update Asset Bundles Target Location
                so.FindProperty("assetBundleFolder").stringValue = newDirectory;
                so.ApplyModifiedProperties();

                mAssetBundleManager.LoadAssetBundle(newDirectory, newTarget.ToString());
                mAssetBundleManager.ReloadUntrackedAssetBundles();
            });
            
            // Generate Asset Bundles Button
            var generateAssetBundlesButton = rootVisualElement.Query<Button>("generate-asset-bundle-button").First();
            generateAssetBundlesButton.clicked += () => {
                Debug.Log("Selected Target: " + mConfig.buildTarget);
                Debug.Log("Folder Asset Bundles: " + mConfig.assetBundleFolder);
                
                CreateAssetBundles.BuildAllAssetBundles(mConfig.buildTarget, mConfig.assetBundleFolder);
                
                // Refresh New Asset Bundles
                mAssetBundleManager.LoadAssetBundle(mConfig.assetBundleFolder, mConfig.buildTarget.ToString());
                mAssetBundleManager.ReloadUntrackedAssetBundles();
            };
            
            // Helpbox holder
            mHelpBoxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();

            // Load Manager UI
            mAssetBundleManager.LoadUI(rootVisualElement);
            
            // Load Manager UI
            mUploadManager.LoadUI(rootVisualElement);

            // Load Viewer
            mViewer.LoadUI(rootVisualElement);
        }

        public void ClearHelpbox()
        {
            mHelpBoxHolder.Clear();
        }

        public void AddErrorHelpbox(string errorMsg)
        {
            mHelpBoxHolder.Add(new HelpBox(errorMsg, HelpBoxMessageType.Error));
        }
    }
}