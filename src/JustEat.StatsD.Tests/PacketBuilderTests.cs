using System.Linq;
using NUnit.Framework;

namespace JustEat.StatsD.Tests
{
    [TestFixture]
    public class WhenBuildingPackets
    {
        private byte[][] _bytes;

        [OneTimeSetUp]
        protected void SetBytes()
        {
            _bytes = new[] {Enumerable.Repeat("a", 512).ToString()}.ToMaximumBytePackets().ToArray();
        }

        [Test]
        public void TheMetricShouldGetSent()
        {
            Assert.That(_bytes[0].Length, Is.InRange(1, 512));
        }
    }
}
