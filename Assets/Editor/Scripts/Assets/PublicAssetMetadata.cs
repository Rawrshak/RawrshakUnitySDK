
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using Rawrshak;

namespace Rawrshak
{
    // Todo: Utility.FromJson() only supports plain object. You need to create a scriptable object AND a
    //       plain object here.
    [Serializable]
    public class PublicAssetMetadata : ScriptableObject
    {
        public string name;
        // public string description;
        // public string image;
        [NonSerialized]
        public AssetType type;
        [NonSerialized]
        public AssetSubtype subtype;
        // public string deploymentDate;
        // public List<PublicLocalizationData> localization;
        [NonSerialized]
        public string fileName;
        
        public void Init(AssetType type, AssetSubtype subtype)
        {
            name = "Asset Name";
            // description = String.Empty;
            // image = String.Empty;
            // this.type = type;
            // this.subtype = subtype;
            // deploymentDate = String.Empty;
            // localization = new List<PublicLocalizationData>();
            fileName = String.Empty;
        }

        public static PublicAssetMetadata CreateFromJSON(string publicMetadataJson)
        {
            TextAsset jsonString = Resources.Load(publicMetadataJson) as TextAsset;
            Debug.Log("Public Metadata JSON: " + publicMetadataJson);
            if (jsonString == null)
            {
                Debug.Log("Public Metadata Json file is null");
                return null;
            }
            Debug.Log("Public Metadata JSON: " + jsonString.text);
            return JsonUtility.FromJson<PublicAssetMetadata>(jsonString.text);
        }

        
        public void SaveToJSON(string filePath)
        {
            string data = JsonUtility.ToJson(this);
            
            // Delete file and .meta filefirst
            File.Delete(filePath);
            File.Delete(filePath + ".meta");
            
            // write file 
            StreamWriter writer = new StreamWriter(filePath, true);
            writer.WriteLine(data);
            writer.Close();
            AssetDatabase.Refresh();
        }

        public void OnDisable()
        {
            string publicMetadataName = String.Format("Public{0}", this.GetInstanceID());
            SaveToJSON(String.Format("{0}/{1}.json", AssetsManager.ASSETS_FILE_LOCATION, publicMetadataName));
        }

        // Todo: we can convert dictionary to string and vice versa.
        //       Or we can use an ExtendoObject and dynamic

        // dynamic obj = new ExpandoObject();
        // obj.Foo = 42;
        // obj.Bar = "hello world";
        // string json = JsonConvert.SerializeObject(obj);
        // Results in: {"Foo":42,"Bar":"hello world"}

        // Deserialize:
        // dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
        // source: https://makolyte.com/csharp-deserialize-json-to-dynamic-object/#:~:text=If%20you%20want%20to%20deserialize%20JSON%20without%20having,can%20use%20this%20object%20like%20any%20other%20object.

    
        // public List<Object> properties;
    }

    [Serializable]
    public class PublicLocalizationData 
    {
        public string locale;
        public string name;
        public string description;

        public PublicLocalizationData()
        {
            locale = String.Empty;
            name = String.Empty;
            description = String.Empty;
        }
    }
}