using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.Localization
{
    [RequireComponent(typeof(Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField, Tooltip("Key to find the JSON")]
        private string key = "Insert Key...";

        private LocalizationManager localizationManager;

        private Text component;

        private void Awake() => component = GetComponent<Text>();

        private void Start()
        {
            localizationManager = LocalizationManager.Instance;
            localizationManager.Subscribe(OnChangeLanguage);
            OnChangeLanguage(localizationManager);
        }

        private void OnDestroy() => localizationManager.Unsubscribe(OnChangeLanguage);

        private void OnChangeLanguage(LocalizationManager localizationManager) => component.text = localizationManager.GetText(key);
    }
}
