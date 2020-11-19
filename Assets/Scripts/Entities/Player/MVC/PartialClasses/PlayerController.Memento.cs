using Asteroids.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController: MonoBehaviour
{



    private readonly struct Memento
    {
        /* This struct is in charge of storing and setting the player state for rewinding
         * Technically, the create and set memento methods should be members of the Originator (PlayerController) class
         * according to the pure Memento pattern.
         * However, that makes a bit convulted the PlayerController class and increase its responsabilities amount.
         * 
         * This is why me add that logic in the Memento type and rewind here. So for the Player's point of view, it's only Memento.TrackForRewind(this).
         * Anyway, the implementation is not exposed because the Memento type is a nested type of the Player class
         * which allow us to access the private state of the Player without exposing it to other non-related classes.
         * 
         * This make easier to organice code.
         * 
         * The following features are not tracked by the memento for gameplay reasons:
         * - Lifes
         * - Invulnerability time
         * - Score
         * 
         * TODO: Remove commit "Refactor Memento pattern" if we regret of this choice.
         */

        // Cache delegate to reduce allocations
        private static readonly Func<Memento, Memento, float, Memento> interpolateMementos = InterpolateMementos;

        private readonly Vector3 position;
        private readonly float rotation;
        private readonly Vector2 velocity;
        private readonly float angularVelocity;

        private Memento(Vector3 position, float rotation, Vector2 velocity, float angularVelocity)
        {
            this.position = position;
            this.rotation = rotation;
            this.velocity = velocity;
            this.angularVelocity = angularVelocity;
        }

        public Memento(PlayerController player) : this(
            player.rigidbody.position,
            player.rigidbody.rotation,
            player.rigidbody.velocity,
            player.rigidbody.angularVelocity
        )
        { }

        public static void TrackForRewind(PlayerController player)
        {
            // We handles anything related with Player's rewind here.

            EventManager.Subscribe<StartRewindEvent>(OnStartRewind);
            EventManager.Subscribe<StopRewindEvent>(OnStopRewind);

            GlobalMementoManager.Subscribe(
                () => new Memento(player),
                (memento) => ConsumeMemento(memento, player),
                interpolateMementos
            );

            void OnStartRewind() => player.collider.enabled = false;

            void OnStopRewind() => player.collider.enabled = true;
        }

        private static void ConsumeMemento(Memento? memento, PlayerController player)
        {
            if (memento is Memento memento_)
                memento_.Load(player);
        }

        public void Load(PlayerController player)
        {
            Rigidbody2D rigidbody = player.rigidbody;
            rigidbody.position = position;
            rigidbody.rotation = rotation;
            rigidbody.velocity = velocity;
            rigidbody.angularVelocity = angularVelocity;
        }

        private static Memento InterpolateMementos(Memento a, Memento b, float delta)
        {
            // Handle screen wrapping
            float height = Camera.main.orthographicSize * 2;
            height *= .9f; // Allow offset error
            if (Mathf.Abs(a.position.y - b.position.y) > height || Mathf.Abs(a.position.x - b.position.x) > height * Camera.main.aspect)
                return delta > .5f ? b : a;

            return new Memento(
                Vector3.Lerp(a.position, b.position, delta),
                Mathf.Lerp(a.rotation, b.rotation, delta),
                Vector2.Lerp(a.velocity, b.velocity, delta),
                Mathf.Lerp(a.angularVelocity, b.angularVelocity, delta)
            );
        }
    }
}

