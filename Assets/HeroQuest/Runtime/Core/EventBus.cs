using System;
using System.Collections.Generic;

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

        public void Publish<TEvent>(TEvent gameEvent)
        {
            if (handlers.TryGetValue(typeof(TEvent), out var handler))
            {
                ((Action<TEvent>)handler)?.Invoke(gameEvent);
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var eventType = typeof(TEvent);
            handlers[eventType] = handlers.TryGetValue(eventType, out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                return;
            }

            var eventType = typeof(TEvent);
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
