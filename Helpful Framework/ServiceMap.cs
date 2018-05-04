using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Helpful.Framework
{
    /// <summary>Not ready for use.</summary>
    internal class ServiceProvider : IServiceProvider
    {
        private ConcurrentDictionary<Type, object> _services { get; }
        private List<ServiceDescriptor> _descriptors { get; }

        public T GetService<T>() => (T)GetService(typeof(T));
        public object GetService(Type serviceType)
        {
            var desc = _descriptors.First(s => s.ServiceType == serviceType);
            switch (desc.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return _services.GetOrAdd(desc.ServiceType, desc.ImplementationFactory(this));
                default:
                    return desc.ImplementationInstance ?? desc.ImplementationFactory(this);
            }
        }

        public IEnumerable<T> GetServices<T>() => GetServices(typeof(T)).Select(o => (T)o);
        public IEnumerable<object> GetServices(Type serviceType)
        {
            foreach (var desc in _descriptors.Where(s => s.ServiceType == serviceType))
            {
                switch (desc.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        yield return _services.GetOrAdd(desc.ServiceType, desc.ImplementationFactory(this));
                        break;
                    default:
                        yield return desc.ImplementationInstance ?? desc.ImplementationFactory(this);
                        break;
                }
            }
        }

        public IEnumerable<T> GetAllServices<T>() => _services.Values.Where(o => o is T).Select(o => (T)o);
        public IEnumerable<object> GetAllServices() => _services.Values;
    }
}
