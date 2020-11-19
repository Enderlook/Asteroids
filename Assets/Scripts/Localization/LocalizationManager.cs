using AvalonStudios.Additions.Extensions;

using System;
using System.Collections.Generic;
using System.IO;

using MiniJSON;

using UnityEngine;

namespace Asteroids.Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Root folder.")]
        private string rootDirectory = "/Resources/Localization";

        public Dictionary<SystemLanguage, Dictionary<string, string>> texts = new Dictionary<SystemLanguage, Dictionary<string, string>>();

        public static LocalizationManager Instance { get; private set; }

        public static SystemLanguage Language { get; private set; } = SystemLanguage.English;


        private void Awake()
        {
            if (Instance.IsNull())
            {
                Instance = this;
                LoadTexts();
            }
            else Destroy(this);
        }

        private void Start()
        {
            Debug.Log("Lenguaje OS: " + Application.systemLanguage);

            Language = Application.systemLanguage;
        }

        public void SwitchLanguage() =>
            Language = Language == SystemLanguage.Spanish ? SystemLanguage.English : SystemLanguage.Spanish;

        private void LoadTexts()
        {
            texts = new Dictionary<SystemLanguage, Dictionary<string, string>>();

            List<string> allFiles = new List<string>();
            string[] getFiles = Directory.GetFiles(Application.dataPath + $"{rootDirectory}/", "*.json", SearchOption.AllDirectories);
            foreach (string file in getFiles)
            {
                string fileName = file.Substring(file.IndexOf("Localization", StringComparison.Ordinal))
                                   .Replace(@"\", @"/");
                allFiles.Add(fileName);
            }

            foreach (string file in allFiles)
            {
                TextAsset asset = Resources.Load<TextAsset>(file.Replace(".json", ""));

                string data = asset.text;

                Dictionary<string, object> parsedData = (Dictionary<string, object>)Json.Deserialize(data);
                string[] split = file.Split('/');

                SetTexts(parsedData, split[split.Length - 1].Replace(".json", ""), split[split.Length - 2]);
            }
        }

        private void SetTexts(Dictionary<string, object> fileContent, string fileName, string language)
        {
            SystemLanguage lang = LanguageMapper.Map(language.ToUpper());

            foreach (var item in fileContent)
            {
                if (!texts.ContainsKey(lang)) texts.Add(lang, new Dictionary<string, string>());

                texts[lang].Add($"{fileName}/{item.Key}", item.Value.ToString());
                Debug.Log($"{lang} --- {fileName}/{item.Key} --- {item.Value}");
            }
        }

        public string GetText(string key)
        {
            if (!texts[Language].ContainsKey(key))
            {
                Debug.LogError($"Key '{key}' for language '{Language}' not found");
                return key;
            }

            return texts[Language][key];
        }

    }
}
