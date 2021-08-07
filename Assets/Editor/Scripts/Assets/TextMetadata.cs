using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using Rawrshak;

namespace Rawrshak
{
    [Serializable]
    public class TextAssetMetadata : ScriptableObject
    {
        public string title;
        public string description;
        public List<TextLocalization> localizations;
        [NonSerialized]
        public string fileName;

        // public List<Object> properties;

        private static int MAX_TITLE_LENGTH = 40;
        private static int MAX_DESCRIPTION_LENGTH = 10000;

        public bool Verify()
        {
            if (!ValidateLength(title, description))
            {
                return false;
            }

            // Check Localization
            foreach(var locale in localizations)
            {
                if (!ValidateLength(locale.title, locale.description))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidateLength(string title, string description)
        {
            return (title.Length > MAX_TITLE_LENGTH && description.Length > MAX_DESCRIPTION_LENGTH);
        }
        

        public static TextAssetMetadata CreateFromJSON(string publicMetadataJson)
        {
            TextAsset jsonString = Resources.Load(publicMetadataJson) as TextAsset;
            Debug.Log("Text Asset Metadata JSON: " + publicMetadataJson);
            if (jsonString == null)
            {
                Debug.Log("Text Asset Metadata Json file is null");
                return null;
            }
            return JsonUtility.FromJson<TextAssetMetadata>(jsonString.text);
        }

        public void SaveToJSON(string fileName)
        {
            string data = JsonUtility.ToJson(this);
            
            // Delete file and .meta filefirst
            File.Delete(fileName);
            File.Delete(fileName + ".meta");
            
            // write file 
            StreamWriter writer = new StreamWriter(fileName, true);
            writer.WriteLine(data);
            writer.Close();
            AssetDatabase.Refresh();
        }

        public void OnDisable()
        {
            string privateMetadataName = String.Format("Private{0}", this.GetInstanceID());
            SaveToJSON(String.Format("{0}/{1}.json", AssetsManager.ASSETS_FILE_LOCATION, privateMetadataName));
        }
    }

    [Serializable]
    public class TextLocalization
    {
        public string locale;
        public string title;
        public string description;
    }

    [Serializable]
    public class TitleMetadata : TextAssetMetadata
    {        
        private static int MAX_DESCRIPTION_LENGTH = 500;
    }

    [Serializable]
    public class LoreMetadata : TextAssetMetadata
    {
        private static int MAX_DESCRIPTION_LENGTH = 5000;
    }

    [Serializable]
    public class CustomTextMetadata : TextAssetMetadata
    { 
        private static int MAX_DESCRIPTION_LENGTH = 10000;
    }
}