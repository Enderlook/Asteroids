using AvalonStudios.Additions.Extensions;
using AvalonStudios.Additions.Utils.Interfaces;
using AvalonStudios.Additions.Attributes;

using System;
using System.Collections.Generic;
using System.IO;

using MiniJSON;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.Localization
{
    public delegate void ChangeLanguageEventHandler(LocalizationManager localizationManager);

    public class LocalizationManager : MonoBehaviour, IEvents<ChangeLanguageEventHandler>
    {
        [SerializeField, Tooltip("Root folder."), ReadOnly]
        private string rootDirectory = "/Resources/Localization";

        public Dictionary<SystemLanguage, Dictionary<string, string>> texts = new Dictionary<SystemLanguage, Dictionary<string, string>>();

        public static LocalizationManager instance = null;
        public static LocalizationManager Instance
        {
            get
            {
                if (instance.IsNull())
                {
                    GameObject obj = new GameObject();
                    obj.name = "Localization Manager";
                    instance = obj.AddComponent<LocalizationManager>();
                }
                return instance;
            }
        }
        
        public static event ChangeLanguageEventHandler OnChangeLanguage;

        public static SystemLanguage Language { get; private set; } = SystemLanguage.English;


        private void Awake()
        {
            if (instance.IsNull())
            {
                instance = this;
                Language = Application.systemLanguage;
                LoadTexts();
                DontDestroyOnLoad(this);
            }
            else Destroy(this);
        }

        private void OnDestroy() => OnChangeLanguage = null;

        public void SwitchLanguage(string lang)
        {
            Language = LanguageMapper.Map(lang.ToUpper());
            if (!OnChangeLanguage.IsNull())
                OnChangeLanguage(this);
        }

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

            foreach (KeyValuePair<string, object> item in fileContent)
            {
                if (!texts.ContainsKey(lang)) texts.Add(lang, new Dictionary<string, string>());

                texts[lang].Add($"{fileName}/{item.Key}", item.Value.ToString());
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

        public void Subscribe(ChangeLanguageEventHandler listener) => OnChangeLanguage += listener;

        public void Unsubscribe(ChangeLanguageEventHandler listener) => OnChangeLanguage -= listener;
    }
}
