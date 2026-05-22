using System;
using System.Collections.Generic;

namespace HeroQuest.Core
{
    public static class ServiceRegistry
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<TService>(TService service) where TService : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Services[typeof(TService)] = service;
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

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
