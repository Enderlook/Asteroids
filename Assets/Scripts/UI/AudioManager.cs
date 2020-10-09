using Asteroids.Events;

using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.UI
{
    [RequireComponent(typeof(SoundPlayer))]
    public class AudioManager : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Index of play music.")]
        private int play;

        [SerializeField, Tooltip("Index of menu music.")]
        private int menu;

        [SerializeField, Tooltip("Index of game over music.")]
        private int gameOver;
#pragma warning restore CS0649

        private SoundPlayer player;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            player = GetComponent<SoundPlayer>();
            EventManager.Subscribe<PauseEvent>(OnPause);
            EventManager.Subscribe<LevelTerminationEvent>(OnLevelTermination);
        }

        private void OnPause(PauseEvent @event) => player.Play(@event.IsPaused ? menu : play);

        private void OnLevelTermination(LevelTerminationEvent @event)
        {
            if (@event.HasLost)
                player.Play(gameOver, () => player.Play(menu));
        }
    }
}
