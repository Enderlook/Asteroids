using System;
using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    public partial class BombWeapon
    {
        public partial class Bomb : MonoBehaviour
        {
            private static readonly List<Vector2> physicsShape = new List<Vector2>();
            public static int totalID;

            public Bomb previous;
            public BombWeapon flyweight;

            // We use this value to maintain order of bombs even during memento or saving
            public int id;
            public int previousId;

            private Transform child;
            private SpriteRenderer spriteRenderer;
            private SpriteRenderer childSpriteRenderer;
            private new PolygonCollider2D collider;
            private AudioSource audioSource;

            private Sprite last;

            private float timer;
            private StateMachine state;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void Start()
            {
                Memento.TrackForRewind(flyweight, this);

                Initialize();

                childSpriteRenderer = child.GetComponent<SpriteRenderer>();
                collider = child.GetComponent<PolygonCollider2D>();

                last = childSpriteRenderer.sprite;
                if (last == null)
                    collider.enabled = false;
                else
                {
                    collider.enabled = true;

                    int count = last.GetPhysicsShapeCount();
                    for (int i = 0; i < count; i++)
                    {
                        last.GetPhysicsShape(i, physicsShape);
                        collider.SetPath(i, physicsShape);
                    }
                }
            }

            public void Initialize()
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                child = transform.GetChild(0);
                audioSource = child.GetComponent<AudioSource>();
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void OnEnable() => id = ++totalID;

            [Serializable]
            private enum StateMachine : byte
            {
                Normal,
                Waiting,
                Exploding
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void Update()
            {
                // This state machine is used to workarround Chain of Responsability during rewind
                // by this way we can rewind and set bombs to explode without remembering
                // the time difference and state between each bomb
                switch (state)
                {
                    case StateMachine.Normal:
                        break;
                    case StateMachine.Waiting:
                        timer -= Time.deltaTime;
                        if (timer <= 0)
                            GotoStateExploding();
                        break;
                    case StateMachine.Exploding:
                        UpdateCollider();
                        if (!audioSource.isPlaying && childSpriteRenderer.sprite == null)
                        {
                            flyweight.builder.Store(this);
                            GotoStateNormal();
                        }
                        break;
                }
            }

            private void GotoStateNormal()
            {
                state = StateMachine.Normal;
                spriteRenderer.enabled = true;
                child.gameObject.SetActive(false);
            }

            private void GotoStateExploding()
            {
                timer = 0;
                state = StateMachine.Exploding;
                spriteRenderer.enabled = false;
                child.gameObject.SetActive(true);
                audioSource.Play();
            }

            private void UpdateCollider()
            {
                Sprite sprite = childSpriteRenderer.sprite;
                if (last != sprite)
                {
                    last = sprite;
                    if (sprite == null)
                        collider.enabled = false;
                    else
                    {
                        collider.enabled = true;

                        int count = sprite.GetPhysicsShapeCount();
                        collider.pathCount = count;
                        for (int i = 0; i < count; i++)
                        {
                            sprite.GetPhysicsShape(i, physicsShape);
                            collider.SetPath(i, physicsShape);
                        }
                    }
                }
            }

            public void Explode(float timeToExplode)
            {
                if (state != StateMachine.Normal)
                {
                    Debug.LogWarning("Possible endless recursion due screwed rewind... chain explosion was terminated and replaced by a fallback.");
                    // Chain of responsability requires each bomb to have a reference to the previous bomb.
                    // However also bombs must use the Pool pattern.
                    // And finally the rewind powerup must go back in time using Memento.
                    // This make quite difficult to track the proper state of a bomb across time and recycles of the same instance
                    // so sometimes the chain gets corrupted and we need to execute this fallback.
                    // The fallback has a different behaviour, but at least it tries to reduce the visibility of the error for the user.
                    flyweight.FallbackExplosion();
                    return;
                }

                state = StateMachine.Waiting;
                timer = timeToExplode;

                if (previous != null)
                {
                    previous.Explode(timeToExplode + flyweight.chainDelay);
                    previous = null;
                }

                if (timer <= 0)
                    GotoStateExploding();
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void OnDrawGizmos()
            {
                if (previous == null)
                    return;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, previous.transform.position);
            }

            public void ConfigurePrevious(Dictionary<int, Bomb> previous, ref bool corrupted)
            {
                // Due to bombs being pooled and work like a linked list, references get invalidated after rewind
                // So we need to store ids and regenerate them after rewind each frame

                if (previousId == 0)
                    this.previous = null;
                else if (previous.TryGetValue(previousId, out Bomb prev))
                    this.previous = prev;
                else
                {
                    Debug.LogWarning("Key not found, state corrupted due to rewind.");
                    this.previous = null;
                    corrupted = true;
                }

                previousId = 0;
            }

            public void FallbackExplosion()
            {
                if (state == StateMachine.Normal)
                    GotoStateExploding();
            }

            public void Reset()
            {
                gameObject.SetActive(false);
                child.gameObject.SetActive(false);
                previous = null;

                // Additional fixes due to rewind, just to be sure
                state = StateMachine.Normal;
                timer = 0;
                audioSource.Stop();
                childSpriteRenderer.sprite = null;
                last = null;
            }
        }
    }
}