using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Events;
using Unity.EditorCoroutines.Editor;

namespace Rawrshak
{
    public class ABViewer : ScriptableObject
    {
        // Singleton instance
        static ABViewer _instance = null;

        ABData mAssetBundle;

        // UI
        Box mHelpBoxHolder;
        Box mViewer;
        
        VisualTreeAsset mBundleTreeAsset;


        public static ABViewer Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<ABViewer>();
                if (!_instance)
                    _instance = ScriptableObject.CreateInstance<ABViewer>();
                return _instance;
            }
        }

        public void SetAssetBundle(ABData assetBundle)
        {
            mAssetBundle = assetBundle;

            mViewer.Clear();

            TemplateContainer bundleTree = mBundleTreeAsset.CloneTree();
            
            var bundleView = bundleTree.contentContainer.Query<Box>("info-box").First();
            SerializedObject so = new SerializedObject(mAssetBundle);
            bundleView.Bind(so);

            mViewer.Add(bundleTree);
        }
        
        public void LoadUI(VisualElement root)
        {
            // Load View UML
            mBundleTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Rawrshak/Editor/UXML/AssetBundleMenu/AssetBundleView.uxml");

            mHelpBoxHolder = root.Query<Box>("helpbox-holder").First();

            // Asset Bundle Entries
            mViewer = root.Query<Box>("asset-bundle-viewer").First();
        }

        public void AddErrorHelpbox(string errorMsg)
        {
            mHelpBoxHolder.Add(new HelpBox(errorMsg, HelpBoxMessageType.Error));
        }

        private IEnumerator QueryUploadCost(ABData bundle)
        {
            // Todo: Implement this when you have the Estimate Cost API
            bundle.mUploadCost = 0.0f;
            yield return null;
        }
    }

}