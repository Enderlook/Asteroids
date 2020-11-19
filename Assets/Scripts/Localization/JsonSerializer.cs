using System.Collections.Generic;
using System.IO;

using MiniJSON;

public static class JsonSerializer {

    public static void SaveJson<T>(this T obj, string absolutePath) {
        string json = Json.Serialize(obj);
        File.WriteAllText(absolutePath, json);
    }

    public static Dictionary<string, object> LoadJson(string absolutePath) {
        string text = File.ReadAllText(absolutePath);
        
        return (Dictionary<string, object>)Json.Deserialize(text);
    }
    
}