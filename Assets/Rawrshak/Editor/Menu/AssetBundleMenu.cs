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
        AssetBundleManager mAssetBundleManager;
        AssetBundleViewer mViewer;

        // UI
        Box mHelpBoxHolder;

        // Static Properties
        public static string ASSET_BUNDLES_FOLDER = "Rawrshak/AssetBundles";
        public static string RESOURCES_FOLDER = "Assets/Rawrshak/Editor/Resources";
        public static string ASSET_BUNDLES_MENU_CONFIG_FOLDER_NAME = "AssetBundlesMenuConfig";
        static string ASSET_BUNDLES_MENU_CONFIG_FILE_NAME = "MenuConfig";
        static Color selectedBGColor = new Color(90f/255f, 90f/255f, 90f/255f, 1f);
        static Color unselectedBGColor = new Color(60f/255f, 60f/255f, 60f/255f, 1f);

        HashSet<SupportedBuildTargets> selectedBuildTargets = new HashSet<SupportedBuildTargets>();

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
            selectedBuildTargets.Clear();

            Debug.Log("AssetBundleMenu Disabled.");
        }

        private void LoadData() {
            // Create Settings folder if necessary
            if(!Directory.Exists(String.Format("{0}/{1}", RESOURCES_FOLDER, ASSET_BUNDLES_MENU_CONFIG_FOLDER_NAME)))
            {
                Directory.CreateDirectory(String.Format("{0}/{1}", RESOURCES_FOLDER, ASSET_BUNDLES_MENU_CONFIG_FOLDER_NAME));
            }

            // Load Rawrshak Settings (and Initialize if necessary)
            mConfig = AssetBundleMenuConfig.Instance;
            mAssetBundleManager = AssetBundleManager.Instance;
            mViewer = AssetBundleViewer.Instance;

            // Set BundleSelected callback
            mAssetBundleManager.SetBundleSelectedCallback(mViewer.SetAssetBundle);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void LoadUXML() {
            var original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Rawrshak/Editor/UXML/AssetBundleMenu/AssetBundleMenu.uxml");
            TemplateContainer treeAsset = original.CloneTree();
            rootVisualElement.Add(treeAsset);
        }

        private void LoadUSS() {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Rawrshak/Editor/USS/AssetBundleMenu/AssetBundleMenu.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void LoadUI() {
            // Build Target enum
            var settingsFoldout = rootVisualElement.Query<Foldout>("settings").First();
            var buildTargetsHolder = rootVisualElement.Query<Box>("build-targets").First();
            
            var buildTargetEntry = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Rawrshak/Editor/UXML/AssetBundleMenu/BuildTargets.uxml");

            // For each build target
            foreach(var buildTarget in Enum.GetValues(typeof(SupportedBuildTargets)))
            {
                TemplateContainer entryTree = buildTargetEntry.CloneTree();
                entryTree.contentContainer.Query<Label>("build-target").First().text = buildTarget.ToString();

                // Set Toggle Callback
                var selectedToggle = entryTree.contentContainer.Query<Toggle>("build-target-selected").First();
                selectedToggle.RegisterCallback<ChangeEvent<bool>>((evt) => {
                    SupportedBuildTargets selected = (SupportedBuildTargets)buildTarget;

                    if (!(evt.target as Toggle).value)
                    {
                        Debug.Log("Deselected: " + selected.ToString());
                        selectedBuildTargets.Remove(selected);
                    }
                    else
                    {
                        Debug.Log("Selected: " + selected.ToString());
                        selectedBuildTargets.Add(selected);
                    }
                });

                entryTree.RegisterCallback<MouseDownEvent>((evt) => {
                    SupportedBuildTargets selected = (SupportedBuildTargets)buildTarget;
                    if (selectedBuildTargets.Contains(selected))
                    {
                        selectedToggle.value = false;
                    }
                    else
                    {
                        selectedToggle.value = true;
                    }
                });
                
                buildTargetsHolder.Add(entryTree);
            }

            SerializedObject so = new SerializedObject(mConfig);
            settingsFoldout.Bind(so);

            var buildTargetEnumField = rootVisualElement.Query<EnumField>("build-target").First();
            buildTargetEnumField.Init(mConfig.selectedBuildTarget);
            buildTargetEnumField.RegisterCallback<ChangeEvent<System.Enum>>((evt) => {
                // Update the Build Target
                var newTarget = (Rawrshak.SupportedBuildTargets)evt.newValue;
                var newDirectory = String.Format("{0}/{1}", ASSET_BUNDLES_FOLDER, newTarget.ToString());
                
                // Update Asset Bundles Target Location
                so.FindProperty("selectedTargetBuildAssetBundleFolder").stringValue = newDirectory;
                so.ApplyModifiedProperties();

                mAssetBundleManager.LoadAssetBundle(newDirectory, newTarget);
                mAssetBundleManager.ReloadUntrackedAssetBundles();
            });
            
            // Generate Asset Bundles Button
            var generateAssetBundlesButton = rootVisualElement.Query<Button>("generate-asset-bundle-button").First();
            generateAssetBundlesButton.clicked += () => {
                // Debug.Log("Selected Target: " + mConfig.selectedBuildTarget);
                // Debug.Log("Folder Asset Bundles: " + mConfig.selectedTargetBuildAssetBundleFolder);
                
                foreach (SupportedBuildTargets buildTarget in selectedBuildTargets)
                {
                    var directory = String.Format("{0}/{1}", ASSET_BUNDLES_FOLDER, buildTarget.ToString());
                    CreateAssetBundles.BuildAllAssetBundles(buildTarget, directory);
                }
                
                // Refresh New Asset Bundles
                mAssetBundleManager.LoadAssetBundle(mConfig.selectedTargetBuildAssetBundleFolder, mConfig.selectedBuildTarget);
                mAssetBundleManager.ReloadUntrackedAssetBundles();
            };
            
            // Helpbox holder
            mHelpBoxHolder = rootVisualElement.Query<Box>("helpbox-holder").First();

            // Load Manager UI
            mAssetBundleManager.LoadUI(rootVisualElement);

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