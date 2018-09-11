namespace JustEat.StatsD.V2
{
    internal readonly struct StatsDMessage
    {
        public readonly string StatBucket;
        public readonly double Magnitude;
        public readonly StatsDMessageKind MessageKind;

        private StatsDMessage(string statBucket, double magnitude, StatsDMessageKind messageKind)
        {
            StatBucket = statBucket;
            Magnitude = magnitude;
            MessageKind = messageKind;
        }

        public static StatsDMessage Timing(long milliseconds, string statBucket)
        {
            return new StatsDMessage(statBucket, milliseconds, StatsDMessageKind.Timing);
        }

        public static StatsDMessage Counter(long magnitude, string statBucket)
        {
            return new StatsDMessage(statBucket, magnitude, StatsDMessageKind.Counter);
        }

        public static StatsDMessage Gauge(double magnitude, string statBucket)
        {
            return new StatsDMessage(statBucket, magnitude, StatsDMessageKind.Gauge);
        }
    }
}
