﻿using Asteroids.Events;

using System.Collections;

using UnityEngine;

namespace Asteroids.Entities.Player
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Collider2D))]
    public class Player : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Amount of lifes the player start with.")]
        private int startingLifes;

        [SerializeField, Tooltip("Maximum amount of lifes the player can have.")]
        private int maxLifes;

        [SerializeField, Tooltip("Duration of invulnerability after lose a life.")]
        private float invulnerabilityDuration;

        [SerializeField, Min(1), Tooltip("Amount of points required to earn a new life. Up to a maximum of one life can be get per score increase.")]
        private int scorePerLife;
#pragma warning restore CS0649

        private static Player instance;

        public static int StartingLifes => instance.startingLifes;

        public static int MaxLifes => instance.maxLifes;

        private int lifes;

        private new Rigidbody2D rigidbody;

        private new Collider2D collider;

        private WaitForSeconds invlunerabilityWait;

        private int scoreToNextLife;

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

            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();

            lifes = startingLifes;
            scoreToNextLife = scorePerLife;

            invlunerabilityWait = new WaitForSeconds(invulnerabilityDuration);

            EventManager.Subscribe<ScoreHasChangedEvent>(OnScoreChanged);
        }

        private void OnScoreChanged(ScoreHasChangedEvent @event)
        {
            if (scoreToNextLife <= @event.NewScore)
            {
                scoreToNextLife += scorePerLife;
                lifes++;
                EventManager.Raise(PlayerHealthChangedEvent.Increase);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (gameObject.layer == LayerMask.NameToLayer("Bullet"))
                return;

            if (lifes == 0)
                EventManager.Raise(LevelTerminationEvent.Lose);
            else
            {
                lifes--;
                EventManager.Raise(PlayerHealthChangedEvent.Decrease);
            }

            rigidbody.position = Vector2.zero;
            rigidbody.rotation = 0;
            collider.enabled = false;

            StartCoroutine(Work());

            IEnumerator Work()
            {
                yield return invlunerabilityWait;
                collider.enabled = true;
            }
        }
    }
}
