using System;

namespace SpaceAce.Architecture
{
    public sealed class CachedService<T>
    {
        private T _service;

        public T Access
        {
            get
            {
                if (_service is null)
                {
                    if (Services.TryGet(out T service) == true) _service = service;
                    else throw new Exception($"Attempted to get unregistered service of type {typeof(T)}!");
                }

                return _service;
            }
        }
    }
}