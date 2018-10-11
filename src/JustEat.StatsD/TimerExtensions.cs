using System;
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
        /// <returns>
        /// An <see cref="IDisposableTimer"/> that publishes the metric when the instance is disposed of.
        /// </returns>
        public static IDisposableTimer StartTimer(this IStatsDPublisher publisher, string bucket)
        {
            return new DisposableTimer(publisher, bucket);
        }

        /// <summary>
        /// Starts a new timer for the specified bucket which is started when the specified delegate is invoked
        /// and is stopped and published when the delegate invocation completes.
        /// </summary>
        /// <param name="publisher">The <see cref="IStatsDPublisher"/> to publish with.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        /// <param name="action">A delegate to a method whose invocation should be timed.</param>
        public static void Time(this IStatsDPublisher publisher, string bucket, Action action)
        {
            using (StartTimer(publisher, bucket))
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
        public static void Time(this IStatsDPublisher publisher, string bucket, Action<IDisposableTimer> action)
        {
            using (var timer = StartTimer(publisher, bucket))
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
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to time.
        /// </returns>
        public static async Task Time(this IStatsDPublisher publisher, string bucket, Func<Task> action)
        {
            using (StartTimer(publisher, bucket))
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
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to time.
        /// </returns>
        public static async Task Time(this IStatsDPublisher publisher, string bucket, Func<IDisposableTimer, Task> action)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                await action(timer).ConfigureAwait(false);
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
        /// <returns>
        /// The value from invoking <paramref name="func"/>.
        /// </returns>
        public static T Time<T>(this IStatsDPublisher publisher, string bucket, Func<T> func)
        {
            using (StartTimer(publisher, bucket))
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
        /// <returns>
        /// The value from invoking <paramref name="func"/>.
        /// </returns>
        public static T Time<T>(this IStatsDPublisher publisher, string bucket, Func<IDisposableTimer, T> func)
        {
            using (var timer = StartTimer(publisher, bucket))
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
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to time.
        /// </returns>
        public static async Task<T> Time<T>(this IStatsDPublisher publisher, string bucket, Func<Task<T>> func)
        {
            using (StartTimer(publisher, bucket))
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
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to time.
        /// </returns>
        public static async Task<T> Time<T>(this IStatsDPublisher publisher, string bucket, Func<IDisposableTimer, Task<T>> func)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                return await func(timer).ConfigureAwait(false);
            }
        }
    }
}
