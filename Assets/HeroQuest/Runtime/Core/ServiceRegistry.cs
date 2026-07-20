using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace HeroQuest.Core
{
    public static class ServiceRegistry
    {
        private static readonly ConcurrentDictionary<Type, object> Services = new();

        public static void Register<TService>(TService service) where TService : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var key = typeof(TService);
            if (Services.TryGetValue(key, out var existing))
            {
                Debug.LogWarning($"[ServiceRegistry] Overwriting existing registration for {key.Name} (old={existing?.GetType().Name}).");
            }
            Services[key] = service;
        }

        public static bool TryResolve<TService>(out TService service) where TService : class
        {
            if (Services.TryGetValue(typeof(TService), out var value))
            {
                service = (TService)value;
                return true;
            }

            service = null;
            return false;
        }

        public static TService Resolve<TService>() where TService : class
        {
            if (TryResolve<TService>(out var service))
            {
                return service;
            }

            throw new InvalidOperationException($"Service is not registered: {typeof(TService).Name}");
        }

        public static bool Unregister<TService>() where TService : class
        {
            return Services.TryRemove(typeof(TService), out _);
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
