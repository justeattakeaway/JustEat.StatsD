using System;

namespace JustEat.StatsD
{
    /// <summary>
    ///     Provides a simple set of extension on DateTime.
    /// </summary>
    public static class DateTimeExtensions
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>	Returns the DateTime as the number of seconds since the epoch (1970), which is Unix time. </summary>
        /// <param name="dateTime">	The dateTime to act on. </param>
        /// <returns>	A number of seconds since the epoch. </returns>
        public static long AsUnixTime(this DateTime dateTime)
        {
            return (long)Math.Round(dateTime.ToUniversalTime().Subtract(Epoch).TotalSeconds);
        }
    }
}
