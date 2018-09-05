namespace JustEat.StatsD.V2
{
    internal readonly struct StatsDMessage
    {
        public readonly string StatBucket;
        public readonly double Magnitude;
        public readonly Kind MessageKind;

        private StatsDMessage(string statBucket, double magnitude, Kind messageKind)
        {
            StatBucket = statBucket;
            Magnitude = magnitude;
            MessageKind = messageKind;
        }

        public static StatsDMessage Timing(long milliseconds, string statBucket)
        {
            return new StatsDMessage(statBucket, milliseconds, Kind.Timing);
        }

        public static StatsDMessage Counter(long magnitude, string statBucket)
        {
            return new StatsDMessage(statBucket, magnitude, Kind.Counter);
        }

        public static StatsDMessage Gauge(double magnitude, string statBucket)
        {
            return new StatsDMessage(statBucket, magnitude, Kind.Gauge);
        }

        internal enum Kind { Counter, Timing, Gauge }
    }
}
