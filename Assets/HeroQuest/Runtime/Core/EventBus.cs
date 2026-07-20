using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroQuest.Core
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent gameEvent);
        void Subscribe<TEvent>(Action<TEvent> handler);
        void Unsubscribe<TEvent>(Action<TEvent> handler);
    }

    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> handlers = new();
        private readonly object lockObj = new();

        public void Publish<TEvent>(TEvent gameEvent)
        {
            Delegate handler;
            lock (lockObj)
            {
                if (!handlers.TryGetValue(typeof(TEvent), out handler))
                    return;
            }

            var invocationList = ((Action<TEvent>)handler)?.GetInvocationList();
            if (invocationList == null) return;

            foreach (var del in invocationList)
            {
                try
                {
                    ((Action<TEvent>)del).Invoke(gameEvent);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] Handler for {typeof(TEvent).Name} threw: {ex}");
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var eventType = typeof(TEvent);
            lock (lockObj)
            {
                handlers[eventType] = handlers.TryGetValue(eventType, out var existing)
                    ? Delegate.Combine(existing, handler)
                    : handler;
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                return;
            }

            var eventType = typeof(TEvent);
            lock (lockObj)
            {
                if (!handlers.TryGetValue(eventType, out var existing))
                {
                    return;
                }

                var next = Delegate.Remove(existing, handler);
                if (next == null)
                {
                    handlers.Remove(eventType);
                    return;
                }
                handlers[eventType] = next;
            }
        }
    }
}
