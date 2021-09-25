namespace JustEat.StatsD.Buffered
{
    internal readonly struct StatsDMessage
    {
        public readonly string StatBucket;
        public readonly double Magnitude;
        public readonly StatsDMessageKind MessageKind;
        public readonly Dictionary<string, string?>? Tags;

        private StatsDMessage(
            string statBucket,
            double magnitude,
            StatsDMessageKind messageKind,
            Dictionary<string, string?>? tags)
        {
            StatBucket = statBucket;
            Magnitude = magnitude;
            MessageKind = messageKind;
            Tags = tags;
        }

        public static StatsDMessage Timing(
            long milliseconds,
            string statBucket,
            Dictionary<string, string?>? tags)
        {
            return new StatsDMessage(statBucket, milliseconds, StatsDMessageKind.Timing, tags);
        }

        public static StatsDMessage Counter(
            long magnitude,
            string statBucket,
            Dictionary<string, string?>? tags)
        {
            return new StatsDMessage(statBucket, magnitude, StatsDMessageKind.Counter, tags);
        }

        public static StatsDMessage Gauge(
            double magnitude,
            string statBucket,
            Dictionary<string, string?>? tags)
        {
            return new StatsDMessage(statBucket, magnitude, StatsDMessageKind.Gauge, tags);
        }
    }
}
