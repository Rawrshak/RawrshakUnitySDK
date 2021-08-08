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
        public AssetBundleMenuConfig mConfig;

        // UI 
        Box mHelpBoxHolder;

        // Static Properties
        static string ASSET_BUNDLES_FOLDER = "AssetBundles";
        static string RESOURCES_FOLDER = "Assets/Editor/Resources";
        static string ASSET_BUNDLES_MENU_CONFIG_DIRECTORY = "AssetBundlesMenuConfig";
        static string ASSET_BUNDLES_MENU_CONFIG_FILE = "config";
        static Color selectedBGColor = new Color(90f/255f, 90f/255f, 90f/255f, 1f);
        static Color unselectedBGColor = new Color(60f/255f, 60f/255f, 60f/255f, 1f);


        [MenuItem("Rawrshak/Asset Bundles")]
        public static void ShowExample()
        {
            AssetBundleMenu wnd = GetWindow<AssetBundleMenu>();
            wnd.titleContent = new GUIContent("Asset Bundles");
        }

        public void OnEnable() {
            LoadData();

            LoadUXML();
            LoadUSS();

            LoadContent();
            Debug.Log("AssetBundleMenu Enabled.");
        }

        public void OnDisable() {
            AssetDatabase.SaveAssets();
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
            AssetDatabase.SaveAssets();
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

        private void LoadContent() {
            // Build Target enum
            var settingsFoldout = rootVisualElement.Query<Foldout>("settings").First();
            var buildTargetEnumField = rootVisualElement.Query<EnumField>("build-target").First();

            SerializedObject so = new SerializedObject(mConfig);
            settingsFoldout.Bind(so);

            buildTargetEnumField.Init(mConfig.buildTarget);
            buildTargetEnumField.RegisterCallback<ChangeEvent<System.Enum>>((evt) => {
                // // Update the Build Target
                var newTarget = (Rawrshak.SupportedBuildTargets)evt.newValue;
                
                // Update Asset Bundles Target Location
                so.FindProperty("assetBundleFolder").stringValue = String.Format("{0}/{1}", ASSET_BUNDLES_FOLDER, newTarget.ToString());
                so.ApplyModifiedProperties();
            });
            
            // Generate Asset Bundles Button
            var generateAssetBundlesButton = rootVisualElement.Query<Button>("generate-asset-bundle-button").First();
            generateAssetBundlesButton.clicked += () => {
                Debug.Log("Selected Target: " + mConfig.buildTarget);
                Debug.Log("Folder Asset Bundles: " + mConfig.assetBundleFolder);
                
                // CreateAssetBundles.BuildAllAssetBundles(mConfig.buildTarget, folderName);
                
                // Todo: Refresh UI
                // Refresh();
            };
            
            // Helpbox holder
            mHelpBoxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();
        }
    }
}