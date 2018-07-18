using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace JustEat.StatsD
{
    /// <summary>	A class that provides simple thread-safe object pooling semantics.  </summary>
    internal sealed class SimpleObjectPool<T>
        where T : class
    {
        private readonly ConcurrentBag<T> _pool;

        /// <summary>	Constructor that populates a pool with the given number of items. </summary>
        /// <exception cref="ArgumentNullException">	Thrown when the constructor is null. </exception>
        /// <exception cref="ArgumentException">		Thrown when the constructor produces null objects. </exception>
        /// <param name="capacity">   	The capacity. </param>
        /// <param name="constructor">	The factory method used to create new instances of the object to populate the pool. </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Necessary to nest generics to support passing a factory method")]
        public SimpleObjectPool(int capacity, Func<SimpleObjectPool<T>, T> constructor)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }

            _pool = new ConcurrentBag<T>();
            for (var i = 0; i < capacity; ++i)
            {
                var instance = constructor(this);
                if (instance == null)
                {
                    throw new ArgumentException("constructor produced null object", "constructor");
                }

                _pool.Add(instance);
            }
        }

        /// <summary>	Retrieves an object from the pool if one is available. </summary>
        /// <returns>	An object or null if the pool has been exhausted. </returns>
        public T Pop()
        {
            if (!_pool.TryTake(out T result))
            {
                result = null;
            }

            return result;
        }

        /// <summary>	Pushes an object back into the pool. </summary>
        /// <exception cref="ArgumentNullException">	Thrown when the item is null. </exception>
        /// <param name="item">	The T to push. </param>
        public void Push(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Items added to a SimpleObjectPool cannot be null");
            }

            _pool.Add(item);
        }
    }
}
