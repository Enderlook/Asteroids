using Asteroids.Scene;

using System;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    public partial class BombWeapon
    {
        public partial class Bomb
        {
            [Serializable]
            private readonly struct Memento
            {
                /* This struct is in charge of storing and setting the bombs state for rewinding
                 * Technically, the create and set memento methods should be members of the Originator class
                 * according to the pure Memento pattern.
                 * However, that makes a bit convulted the BombWeapon class and increase its responsabilities amount.
                 * 
                 * This is why me add that logic in the Memento type and rewind here. So for the BombWeapon's point of view, it's only Memento.TrackForRewind.
                 * Anyway, the implementation is not exposed because the Memento type is a nested type of the BombWeapon class
                 * which allow us to access the private state of the BombWeapon without exposing it to other non-related classes.
                 * 
                 * This make easier to organice code.
                 */

                // Cache delegate to reduce allocations
                private static readonly Func<Memento, Memento, float, Memento> interpolateMementos = InterpolateMementos;

                private readonly bool enabled;
                private readonly SerializableVector2 position;
                private readonly float rotation;
                private readonly int id;
                private readonly int previous;
                private readonly float timer;
                private readonly StateMachine state;

                public Memento(Bomb bomb)
                {
                    enabled = bomb.gameObject.activeSelf;
                    position = (Vector2)bomb.gameObject.transform.position;
                    rotation = bomb.gameObject.transform.rotation.eulerAngles.z;
                    id = bomb.id;
                    previous = bomb.previous?.id ?? 0;

                    state = bomb.state;
                    switch (state)
                    {
                        case StateMachine.Normal:
                            timer = 0;
                            break;
                        case StateMachine.Waiting:
                            timer = bomb.timer;
                            break;
                        case StateMachine.Exploding:
                            Transform child = bomb.transform.GetChild(0);
                            timer = child.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime;
                            break;
                        default:
                            timer = 0;
                            Debug.LogError("Impossible state.");
                            break;
                    }
                }

                public Memento(bool enabled, Vector2 position, float rotation, int id, int previous, StateMachine state, float timer)
                {
                    this.enabled = enabled;
                    this.position = position;
                    this.rotation = rotation;
                    this.id = id;
                    this.previous = previous;
                    this.state = state;
                    this.timer = timer;
                }

                public static void TrackForRewind(BombWeapon bombWeapon, Bomb bomb) => GlobalMementoManager.Subscribe(
                        () => new Memento(bomb),
                        (memento) => ConsumeMemento(memento, bombWeapon, bomb),
                        interpolateMementos
                    );

                private static void ConsumeMemento(Memento? memento, BombWeapon bombWeapon, Bomb bomb)
                {
                    if (memento is Memento memento_)
                        memento_.Load(bombWeapon, bomb);
                    else if (bomb.gameObject.activeSelf) // Don't pool something already pooled
                        bombWeapon.builder.Store(bomb);
                }

                public void Load(BombWeapon bombWeapon, Bomb bomb)
                {
                    if (enabled)
                    {
                        // Since bombs are pooled, we must force the pool to give us control of this instance in case it was in his control.
                        bombWeapon.builder.ExtractIfHas(bomb);

                        bomb.transform.position = (Vector2)position;
                        bomb.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
                        bomb.id = id;
                        bomb.previousId = previous;
                        bomb.previous = null;

                        bomb.GotoStateNormal();
                        switch (state)
                        {
                            case StateMachine.Normal:
                                break;
                            case StateMachine.Waiting:
                                bomb.Explode(timer);
                                break;
                            case StateMachine.Exploding:
                                bomb.Explode(0);
                                Animator animator = bomb.transform.GetChild(0).GetComponent<Animator>();
                                animator.enabled = false;
                                animator.enabled = true;
                                animator.Play(0, 0, timer);
                                break;
                        }
                    }
                    else if (bomb.gameObject.activeSelf) // Don't pool something already pooled
                        bombWeapon.builder.Store(bomb);
                }

                private static Memento InterpolateMementos(
                    Memento a,
                    Memento b,
                    float delta
                    )
                {
                    StateMachine state;
                    float timer;
                    switch (a.state)
                    {
                        case StateMachine.Normal:
                            if (b.state == StateMachine.Normal)
                            {
                                state = StateMachine.Normal;
                                timer = 0;
                            }
                            else
                            {
                                if (delta < .5f)
                                {
                                    state = StateMachine.Normal;
                                    timer = 0;
                                }
                                else
                                {
                                    state = b.state;
                                    timer = b.timer;
                                }
                            }
                            break;
                        case StateMachine.Waiting:
                            if (b.state == StateMachine.Waiting)
                            {
                                state = StateMachine.Waiting;
                                timer = Mathf.Lerp(a.timer, b.timer, delta);
                            }
                            else
                            {
                                if (delta < .5f)
                                {
                                    state = StateMachine.Waiting;
                                    timer = a.timer;
                                }
                                else
                                {
                                    state = b.state;
                                    timer = b.timer;
                                }
                            }
                            break;
                        case StateMachine.Exploding:
                            if (b.state == StateMachine.Exploding)
                            {
                                state = StateMachine.Exploding;
                                timer = Mathf.Lerp(a.timer, b.timer, delta);
                            }
                            else
                            {
                                if (delta < .5f)
                                {
                                    state = StateMachine.Exploding;
                                    timer = a.timer;
                                }
                                else
                                {
                                    state = b.state;
                                    timer = b.timer;
                                }
                            }
                            break;
                        default:
                            state = StateMachine.Normal;
                            timer = 0;
                            Debug.LogError("Impossible state.");
                            break;
                    }

                    return new Memento(
                        delta < .5f ? a.enabled : b.enabled,
                        delta < .5f ? a.position : b.position,
                        delta < .5f ? a.rotation : b.rotation,
                        delta < .5f ? a.id : b.id,
                        delta < .5f ? a.previous : b.previous,
                        state,
                        timer
                    );
                }
            }
        }
    }
}