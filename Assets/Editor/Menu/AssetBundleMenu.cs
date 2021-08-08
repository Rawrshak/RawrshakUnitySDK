using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rawrshak
{
    public class AssetBundleMenu : EditorWindow
    {
        // Private Menu Properties
        SupportedBuildTargets mSelectedBuildTarget = SupportedBuildTargets.StandaloneWindows;
        string mFolderLocation = "AssetBundles/StandaloneWindows";

        // UI 
        Box mHelpBoxHolder;

        // Static Properties
        static string ASSET_BUNDLES_FOLDER = "AssetBundles";
        static Color selectedBGColor = new Color(90f/255f, 90f/255f, 90f/255f, 1f);
        static Color unselectedBGColor = new Color(60f/255f, 60f/255f, 60f/255f, 1f);


        [MenuItem("Rawrshak/Asset Bundle Menu")]
        public static void ShowExample()
        {
            AssetBundleMenu wnd = GetWindow<AssetBundleMenu>();
            wnd.titleContent = new GUIContent("AssetBundleMenu");
        }

        public void OnEnable() {
            LoadData();

            LoadUXML();
            LoadUSS();

            LoadContent();
            Debug.Log("AssetBundleMenu Enabled.");
        }

        public void OnDisable() {
            Debug.Log("AssetBundleMenu Disabled.");
        }

        private void LoadData() {
            // Load Settings
            // Todo: 
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
            var buildTargetEnumField = rootVisualElement.Query<EnumField>("build-target").First();
            buildTargetEnumField.Init(mSelectedBuildTarget);
            buildTargetEnumField.RegisterCallback<ChangeEvent<System.Enum>>((evt) => {
                // Update the Build Target
                mSelectedBuildTarget = (Rawrshak.SupportedBuildTargets)evt.newValue;
                
                // Update Asset Bundles Target Location
                mFolderLocation = String.Format("{0}/{1}", ASSET_BUNDLES_FOLDER, mSelectedBuildTarget.ToString());
            });
            
            // Generate Asset Bundles Button
            var generateAssetBundlesButton = rootVisualElement.Query<Button>("generate-asset-bundle-button").First();
            generateAssetBundlesButton.clicked += () => {
                Debug.Log("Selected Target: " + mSelectedBuildTarget);
                Debug.Log("Folder Asset Bundles: " + mFolderLocation);
                
                // CreateAssetBundles.BuildAllAssetBundles(mSelectedBuildTarget, folderName);
                
                // Todo: Refresh UI
                // Refresh();
            };
            
            // Helpbox holder
            mHelpBoxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();
        }
    }
}