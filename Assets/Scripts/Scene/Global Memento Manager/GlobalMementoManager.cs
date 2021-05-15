using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Asteroids.Scene
{
    [DefaultExecutionOrder((int)ExecutionOrder.O1_GlobalMementoManager)]
    public sealed partial class GlobalMementoManager : MonoBehaviour
    {
        private static GlobalMementoManager instance;

        private const float expirationTime = 10; // Two rewind power up can appear very close
        private const float rewindTime = 5;
        private const int storePerSecond = 5;
        private static readonly int aproximateStoredmementos = Mathf.CeilToInt(storePerSecond * expirationTime);

        private static WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        public static bool IsRewinding => instance.stopAt > Time.fixedTime;

#pragma warning disable CS0649
        [SerializeField]
        private PostProcessVolume volume;
#pragma warning restore CS0649

        private List<IMementoManager> managers = new List<IMementoManager>();
        private float stopAt;
        private float speed;
        private float toStore;
        private const float storeCooldown = 1f / storePerSecond;
        private float deltaRewind;
        private const float volumeSpeed = 5;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"Can only have a single instance of {nameof(GlobalMementoManager)}.");
                Destroy(this);
            }
            else
                instance = this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R))
                Rewind(3);

            if (IsRewinding)
            {
                // Rewind updates must be the same as fixed updates or the effect looks odd
                volume.weight = Mathf.Min(volume.weight + Time.fixedDeltaTime * volumeSpeed, 1);
                foreach (IMementoManager manager in managers)
                    manager.UpdateRewind(Time.fixedDeltaTime);
            }
            else
            {
                if (!Physics.autoSimulation)
                {
                    Physics.autoSimulation = true;
                    EventManager.Raise(new StopRewindEvent());
                }

                volume.weight = Mathf.Max(volume.weight - Time.fixedDeltaTime * volumeSpeed, 0);

                if (storePerSecond == 50) // Physics updates are 50 per second unless manually changed, which is not our case
                    foreach (IMementoManager manager in managers)
                        manager.Store();
                else
                {
                    toStore -= Time.fixedDeltaTime;
                    if (toStore < 0)
                    {
                        toStore = storeCooldown - toStore;
                        foreach (IMementoManager manager in managers)
                            manager.Store();
                    }
                }
            }
        }

        public static void Rewind(float duration)
        {
            if (instance.stopAt < Time.fixedTime) // Ignore rewind if we are rewinding
            {
                instance.stopAt = Time.fixedTime + rewindTime;
                Physics.autoSimulation = false;
                EventManager.Raise(new StartRewindEvent());
                instance.speed = rewindTime / duration;
                foreach (IMementoManager manager in instance.managers)
                    manager.StartRewind();
            }
        }

        public static void Subscribe<T>(Func<T> onStore, Action<T?> onRewind, Func<T, T, float, T> interpolate) where T : struct
        {
            // We don't require to remove callbacks never because this object and its originators are stop being used at the same time

            MementoManager<T> manager = new MementoManager<T>(onStore, onRewind, interpolate);
            instance.managers.Add(manager);
        }
    }

    public readonly struct StartRewindEvent { }

    public readonly struct StopRewindEvent { }
}