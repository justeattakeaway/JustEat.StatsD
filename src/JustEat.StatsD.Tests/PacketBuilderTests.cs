using System.Linq;
using Shouldly;
using Xunit;

namespace JustEat.StatsD.Tests
{
    public class WhenBuildingPackets
    {
        private byte[][] _bytes;

        public WhenBuildingPackets()
        {
            _bytes = new[] { Enumerable.Repeat("a", 512).ToString()}.ToMaximumBytePackets().ToArray();
        }

        [Fact]
        public void TheMetricShouldGetSent()
        {
            _bytes[0].Length.ShouldBeInRange(1, 512);
        }
    }
}
