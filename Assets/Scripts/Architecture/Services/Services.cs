using System;
using System.Collections.Generic;

namespace SpaceAce.Architecture
{
    public static class Services
    {
        private static readonly Dictionary<Type, object> s_registeredServices = new();

        public static void Register(object service)
        {
            if (service is null)
                throw new ArgumentNullException(nameof(service),
                    "Attempted to register an empty service!");

            Type serviceType = service.GetType();

            if (s_registeredServices.ContainsKey(serviceType))
                throw new Exception($"Attempted to register a duplicate service of type {serviceType}!");

            s_registeredServices.Add(serviceType, service);
        }

        public static bool TryGet<T>(out T service)
        {
            if (s_registeredServices.TryGetValue(typeof(T), out var value) == true)
            {
                service = (T)value;
                return true;
            }

            service = default;
            return false;
        }

        public static bool TryGet<T>(out IEnumerable<T> services)
        {
            List<T> targetServices = new(s_registeredServices.Count);

            foreach (var entry in s_registeredServices)
                if (entry.Value is T service)
                    targetServices.Add(service);

            if (targetServices.Count > 0)
            {
                services = targetServices;
                return true;
            }

            services = null;
            return false;
        }

        public static void Clear() => s_registeredServices.Clear();
    }
}