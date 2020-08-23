using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class containing timing extension methods for the <see cref="IStatsDPublisher"/> interface. This class cannot be inherited.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TimerExtensions
    {
        /// <summary>
        /// Starts a new timer for the specified bucket which is published when the return value is disposed of.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="tags">The tags to publish with the timer.</param>
        /// <returns>
        /// An <see cref="IDisposableTimer"/> that publishes the metric when the instance is disposed of.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/> or <paramref name="bucket"/> is <see langword="null"/>.
        /// </exception>
        public static IDisposableTimer StartTimer(
            this IStatsDPublisher publisher,
            string bucket,
            IDictionary<string, string?>? tags = null)
        {
            return new DisposableTimer(publisher, bucket, tags);
        }

        /// <summary>
        /// Starts a new timer for the specified bucket which is started when the specified delegate is invoked
        /// and is stopped and published when the delegate invocation completes.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="action">A delegate to a method whose invocation should be timed.</param>
        /// <param name="tags">The tags to publish with the timer.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/>, <paramref name="bucket"/> or <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public static void Time(
            this IStatsDPublisher publisher,
            string bucket,
            Action action,
            IDictionary<string, string?>? tags = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (StartTimer(publisher, bucket, tags))
            {
                action();
            }
        }

        /// <summary>
        /// Starts a new timer for the specified bucket which is started when the specified delegate is invoked
        /// and is stopped and published when the delegate invocation completes.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="action">A delegate to a method whose invocation should be timed.</param>
        /// <param name="tags">The tags to publish with the timer.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/>, <paramref name="bucket"/> or <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public static void Time(
            this IStatsDPublisher publisher,
            string bucket,
            Action<IDisposableTimer> action,
            IDictionary<string, string?>? tags = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var timer = StartTimer(publisher, bucket, tags))
            {
                action(timer);
            }
        }

        /// <summary>
        /// Starts a new timer for the specified bucket which is started when the specified delegate is invoked
        /// and awaited, and is stopped and published when the asynchronous delegate invocation completes.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="action">A delegate to a method whose invocation should be timed.</param>
        /// <param name="tags">The tags to publish with the timer.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to time.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/>, <paramref name="bucket"/> or <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public static async Task Time(
            this IStatsDPublisher publisher,
            string bucket,
            Func<Task> action,
            IDictionary<string, string?>? tags = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (StartTimer(publisher, bucket, tags))
            {
                await action().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Starts a new timer for the specified bucket which is started when the specified delegate is invoked
        /// and awaited, and is stopped and published when the asynchronous delegate invocation completes.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="action">A delegate to a method whose invocation should be timed.</param>
        /// <param name="tags">The tags to publish with the timer.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to time.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/>, <paramref name="bucket"/> or <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public static async Task Time(
            this IStatsDPublisher publisher,
            string bucket,
            Func<IDisposableTimer, Task> action,
            IDictionary<string, string?>? tags = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var timer = StartTimer(publisher, bucket, tags))
            {
                await action(timer).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Starts a new timer for the specified bucket and tags which is started when the specified delegate is invoked
        /// and is stopped and published when the delegate invocation completes.
        /// </summary>
        /// <typeparam name="T">The type of the result of the delegate to invoke.</typeparam>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="func">A delegate to a method whose invocation should be timed and result returned.</param>
        /// <param name="tags">The tags to publish with the timer.</param>
        /// <returns>
        /// The value from invoking <paramref name="func"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/>, <paramref name="bucket"/>, <paramref name="tags"/> or <paramref name="func"/> is <see langword="null"/>.
        /// </exception>
        public static T Time<T>(
            this IStatsDPublisher publisher,
            string bucket,
            Func<T> func,
            IDictionary<string, string?>? tags = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (StartTimer(publisher, bucket, tags))
            {
                return func();
            }
        }

        /// <summary>
        /// Starts a new timer for the specified bucket which is started when the specified delegate is invoked
        /// and is stopped and published when the delegate invocation completes.
        /// </summary>
        /// <typeparam name="T">The type of the result of the delegate to invoke.</typeparam>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="func">A delegate to a method whose invocation should be timed and result returned.</param>
        /// <param name="tags">The tags to publish with the timer.</param>
        /// <returns>
        /// The value from invoking <paramref name="func"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/>, <paramref name="bucket"/> or <paramref name="func"/> is <see langword="null"/>.
        /// </exception>
        public static T Time<T>(
            this IStatsDPublisher publisher,
            string bucket,
            Func<IDisposableTimer, T> func,
            IDictionary<string, string?>? tags = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (var timer = StartTimer(publisher, bucket, tags))
            {
                return func(timer);
            }
        }

        /// <summary>
        /// Starts a new timer for the specified bucket which is started when the specified delegate is invoked
        /// and awaited, and is stopped and published when the asynchronous delegate invocation completes.
        /// </summary>
        /// <typeparam name="T">The type of the result of the delegate to invoke.</typeparam>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="func">A delegate to a method whose invocation should be timed and result returned.</param>
        /// <param name="tags">The tags to publish with the timer.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to time.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/>, <paramref name="bucket"/> or <paramref name="func"/> is <see langword="null"/>.
        /// </exception>
        public static async Task<T> Time<T>(
            this IStatsDPublisher publisher,
            string bucket,
            Func<Task<T>> func,
            IDictionary<string, string?>? tags = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (StartTimer(publisher, bucket, tags))
            {
                return await func().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Starts a new timer for the specified bucket which is started when the specified delegate is invoked
        /// and awaited, and is stopped and published when the asynchronous delegate invocation completes.
        /// </summary>
        /// <typeparam name="T">The type of the result of the delegate to invoke.</typeparam>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="func">A delegate to a method whose invocation should be timed and result returned.</param>
        /// <param name="tags">The tags to publish with the timer.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to time.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/>, <paramref name="bucket"/> or <paramref name="func"/> is <see langword="null"/>.
        /// </exception>
        public static async Task<T> Time<T>(
            this IStatsDPublisher publisher,
            string bucket,
            Func<IDisposableTimer, Task<T>> func,
            IDictionary<string, string?>? tags = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (var timer = StartTimer(publisher, bucket, tags))
            {
                return await func(timer).ConfigureAwait(false);
            }
        }
    }
}
