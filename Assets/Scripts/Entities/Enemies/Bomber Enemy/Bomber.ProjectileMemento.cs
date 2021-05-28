//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Scene;

using System;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public partial class Bomber
    {
        public partial class Bomb
        {
            [Serializable]
            public readonly struct ProjectileMemento
            {
                /* This struct is in charge of storing and setting the bullets state for rewinding
                    * Technically, the create and set memento methods should be members of the Originator class
                    * according to the pure Memento pattern.
                    * However, that makes a bit convulted the Bomber class and increase its responsabilities amount.
                    * 
                    * This is why me add that logic in the Memento type and rewind here. So for the Bomber's point of view, it's only Memento.TrackForRewind.
                    * Anyway, the implementation is not exposed because the Memento type is a nested type of the Bomber class
                    * which allow us to access the private state of the Bomber without exposing it to other non-related classes.
                    * 
                    * This make easier to organice code.
                    */

                // Cache delegate to reduce allocations
                private static readonly Func<ProjectileMemento, ProjectileMemento, float, ProjectileMemento> interpolateMementos = InterpolateMementos;

                private readonly bool enabled;
                private readonly SerializableVector2 position;
                private readonly float rotation;
                private readonly float timer;

                public ProjectileMemento(Bomb bomb) : this(
                    bomb.gameObject.activeSelf,
                    bomb.transform.position,
                    bomb.transform.rotation.eulerAngles.z,
                    bomb.isExploding ? bomb.animator.GetCurrentAnimatorStateInfo(0).normalizedTime : -1
                )
                { }

                public ProjectileMemento(bool enabled, Vector2 position, float rotation, float timer)
                {
                    this.enabled = enabled;
                    this.position = position;
                    this.rotation = rotation;
                    this.timer = timer;
                }

                public static void TrackForRewind(Bomber shooter, Bomb bomb) => GlobalMementoManager.Subscribe(
                        () => new ProjectileMemento(bomb),
                        (memento) => ConsumeMemento(memento, shooter, bomb),
                        interpolateMementos
                    );

                private static void ConsumeMemento(ProjectileMemento? memento, Bomber shooter, Bomb bomb)
                {
                    if (memento is ProjectileMemento memento_)
                        memento_.Load(shooter, bomb);
                    else if (bomb.gameObject.activeSelf) // Don't pool something already pooled
                        shooter.builder.Store(bomb);
                }

                public void Load(Bomber shooter, Bomb bomb)
                {
                    if (enabled)
                    {
                        // Since bomb are pooled, we must force the pool to give us control of this instance in case it was in his control.
                        shooter.builder.ExtractIfHas(bomb);

                        bomb.transform.position = (Vector2)position;
                        bomb.transform.rotation = Quaternion.Euler(0, 0, rotation);

                        bomb.Initialize();
                        bomb.GotoStateNormal();
                        if (timer != -1)
                        {
                            bomb.GotoStateExploding();
                            Animator animator = bomb.animator;
                            animator.enabled = false;
                            animator.enabled = true;
                            animator.Play(0, 0, timer);
                        }
                    }
                    else if (bomb.gameObject.activeSelf) // Don't pool something already pooled
                        shooter.builder.Store(bomb);
                }

                private static ProjectileMemento InterpolateMementos(
                    ProjectileMemento a,
                    ProjectileMemento b,
                    float delta
                    ) => new ProjectileMemento(
                        delta > .5f ? b.enabled : a.enabled,
                        Vector2.Lerp(a.position, b.position, delta),
                        Mathf.LerpAngle(a.rotation, b.rotation, delta),
                        a.timer == -1 || b.timer == -1 ? (delta > .5f ? b.timer : a.timer) : Mathf.Lerp(a.timer, b.timer, delta)
                    );
            }
        }
    }
}