using UnityEngine;

// Todo: Serialize and load from json file
public class Settings : ScriptableObject {
    public string ethereumUrl;

    public void Awake()
    {
        load();
    }
    public void OnDestroy() {
        save();
    }

    public void load() {
        // Todo: load settings from json file
    }

    public void save() {
        // Todo: save settings to json file
    }
}