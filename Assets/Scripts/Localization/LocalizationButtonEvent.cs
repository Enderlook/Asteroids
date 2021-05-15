using UnityEngine;

namespace Asteroids.Localization
{
    public sealed class LocalizationButtonEvent : MonoBehaviour
    {
        private LocalizationManager localizationManager;

        private void Start() => localizationManager = LocalizationManager.Instance;

        public void ChangeLanguage(string lang) => localizationManager.SwitchLanguage(lang);
    }
}
