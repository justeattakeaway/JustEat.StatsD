using System;
using System.Collections.Concurrent;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class that provides simple thread-safe object pooling semantics.
    /// </summary>
    public sealed class SimpleObjectPool<T> where T : class
    {
        private readonly Func<SimpleObjectPool<T>, T> _constructor;
        private readonly ConcurrentBag<T> _pool;

        /// <summary>	Constructor that populates a pool with the given number of items. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when the constructor is null. </exception>
        /// <param name="initialSize"> Number of items in the pool at start </param>
        /// <param name="constructor"> The factory method used to create new instances of the object to populate the pool. </param>
        public SimpleObjectPool(int initialSize, Func<SimpleObjectPool<T>, T> constructor)
        {
            _constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            _pool = new ConcurrentBag<T>();
            PrePopulate(initialSize);
        }

        private void PrePopulate(int size)
        {
            while (Count < size)
            {
                var instance = _constructor(this);
                if (instance == null)
                {
                    throw new InvalidOperationException("constructor produced null object");
                }

                _pool.Add(instance);
            }
        }

        internal int Count => _pool.Count;

        /// <summary>Retrieves an object from the pool if one is available.
        /// return null if the pool is empty</summary>
        /// <returns>An object from the pool. </returns>
        public T Pop()
        {
            if (_pool.TryTake(out T result))
            {
                return result;
            }

            return null;
        }

        /// <summary>Retrieves an object from the pool if one is available.
        /// Creates a new object if the pool is empty </summary>
        /// <returns>An object from the pool. </returns>
        internal T PopOrCreate()
        {
            return Pop() ?? _constructor(this);
        }

        /// <summary>	Pushes an object back into the pool. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when the item is null.</exception>
        /// <param name="item">The T to push.</param>
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
