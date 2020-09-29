using System;
using System.Collections.Generic;

using UnityEngine;

namespace Asteroids
{
    [DefaultExecutionOrder(-1)]
    public class EventManager : MonoBehaviour
    {
        private static EventManager instance;

        private Dictionary<Event, (Action<object>, Action)> callbacks;

        public enum Event : byte
        {
            PlayerGotNewLife,
            PlayerLostOneLife,
            LevelComplete,
            LostGame,
        }

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
            callbacks = new Dictionary<Event, (Action<object>, Action)>();
        }

        /// <inheritdoc cref="RaiseInstance(Event, object)(Event, object)"/>
        public static void Raise(Event @event, object parameter) => instance.RaiseInstance(@event, parameter);

        /// <inheritdoc cref="RaiseInstance(Event, object)(Event, object)"/>
        public static void Raise(Event @event) => instance.RaiseInstance(@event, null);

        /// <inheritdoc cref="SubscribeInstance(Event, Action{object})"/>
        public static void Subscribe(Event @event, Action<object> callback) => instance.SubscribeInstance(@event, callback);

        /// <inheritdoc cref="SubscribeInstance(Event, Action)"/>
        public static void Subscribe(Event @event, Action callback) => instance.SubscribeInstance(@event, callback);

        /// <inheritdoc cref="UnsubscribeInstance(Event, Action{object})"/>
        public static void Unsubscribe(Event @event, Action<object> callback) => instance.UnsubscribeInstance(@event, callback);

        /// <inheritdoc cref="UnsubscribeInstance(Event, Action)"/>
        public static void Unsubscribe(Event @event, Action callback) => instance.UnsubscribeInstance(@event, callback);

        /// <summary>
        /// Subscribes the callback <paramref name="callback"/> to execute when the event <paramref name="event"/> is raised.
        /// </summary>
        /// <param name="event">Event that executes the callback.</param>
        /// <param name="callback">Callback to execute.</param>
        private void SubscribeInstance(Event @event, Action<object> callback)
        {
            if (callbacks.TryGetValue(@event, out (Action<object>, Action) actions))
            {
                actions.Item1 += callback;
                callbacks[@event] = actions;
            }
            callbacks[@event] = (callback, null);
        }

        /// <inheritdoc cref="SubscribeInstance(Event, Action{object})"/>
        private void SubscribeInstance(Event @event, Action callback)
        {
            if (callbacks.TryGetValue(@event, out (Action<object>, Action) actions))
            {
                actions.Item2 += callback;
                callbacks[@event] = actions;
            }
            callbacks[@event] = (null, callback);
        }

        /// <summary>
        /// Unsubscribes the callback <paramref name="callback"/> from execution when the event <paramref name="event"/> is raised.
        /// </summary>
        /// <param name="event">Event that executes the callback to remove.</param>
        /// <param name="callback">Callback to no longer execute.</param>
        private void UnsubscribeInstance(Event @event, Action<object> callback)
        {
            if (callbacks.TryGetValue(@event, out (Action<object>, Action) actions))
            {
                actions.Item1 -= callback;
                callbacks[@event] = actions;
            }
        }
        
        /// <summary>
        /// Unsubscribes the callback <paramref name="callback"/> from execution when the event <paramref name="event"/> is raised.
        /// </summary>
        /// <param name="event">Event that executes the callback to remove.</param>
        /// <param name="callback">Callback to no longer execute.</param>
        private void UnsubscribeInstance(Event @event, Action callback)
        {
            if (callbacks.TryGetValue(@event, out (Action<object>, Action) actions))
            {
                actions.Item2 -= callback;
                callbacks[@event] = actions;
            }
        }

        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <param name="event">Event to raise.</param>
        /// <param name="parameter">Parameter (if any) of the event.</param>
        private void RaiseInstance(Event @event, object parameter)
        {
            if (callbacks.TryGetValue(@event, out (Action<object>, Action) actions))
            {
                actions.Item1?.Invoke(parameter);
                actions.Item2?.Invoke();
            }
        }
    }
}