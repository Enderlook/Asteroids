using UnityEngine;

namespace Asteroids.Entities.Player
{
    public class Player : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Amount of lifes the player start with.")]
        private int startingLifes;

        [SerializeField, Tooltip("Maximum amount of lifes the player can have.")]
        private int maxLifes;
#pragma warning restore CS0649

        private static Player instance;

        public static int StartingLifes => instance.startingLifes;

        public static int MaxLifes => instance.maxLifes;

        private int lifes;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"{nameof(Player)} can't have more than one instance at the same time.");
                Destroy(gameObject);
                return;
            }
            instance = this;

            lifes = startingLifes;

            EventManager.Subscribe(EventManager.Event.LevelComplete, OnLevelComplete);
        }

        private void OnLevelComplete()
        {
            if (lifes < maxLifes)
            {
                lifes++;
                EventManager.Raise(EventManager.Event.PlayerGotNewLife);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (--lifes == 0)
                EventManager.Raise(EventManager.Event.LostGame);
            else
                EventManager.Raise(EventManager.Event.PlayerLostOneLife);
        }
    }
}
