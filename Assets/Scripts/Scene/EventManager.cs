using Asteroids.Utils;

using System;

using UnityEngine;

namespace Asteroids.Scene
{
    [DefaultExecutionOrder((int)ExecutionOrder.O1_EventManager)]
    public sealed class EventManager : MonoBehaviour
    {
        private static EventManager instance;

        private EventManager<object> manager;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"{nameof(EventManager)} can't have more than one instance at the same time.");
                Destroy(gameObject);
                return;
            }
            instance = this;
            manager = new EventManager<object>();
        }

        /// <inheritdoc cref="EventManager{TEventBase}.Subscribe{TEvent}(Action{TEvent})"/>
        public static void Subscribe<TEvent>(Action<TEvent> callback) => instance.manager.Subscribe(callback);

        /// <inheritdoc cref="EventManager{TEventBase}.Unsubscribe{TEvent}(Action{TEvent})"/>
        public static void Unsubscribe<TEvent>(Action<TEvent> callback) => instance.manager.Unsubscribe(callback);

        /// <inheritdoc cref="EventManager{TEventBase}.Subscribe{TEvent}(Action)"/>
        public static void Subscribe<TEvent>(Action callback) => instance.manager.Subscribe<TEvent>(callback);

        /// <inheritdoc cref="EventManager{TEventBase}.Unsubscribe{TEvent}(Action)"/>
        public static void Unsubscribe<TEvent>(Action callback) => instance.manager.Unsubscribe<TEvent>(callback);

        /// <inheritdoc cref="EventManager{TEventBase}.Raise{TEvent}(TEvent)"/>
        public static void Raise<TEvent>(TEvent eventArgument) => instance.manager.Raise(eventArgument);
    }
}