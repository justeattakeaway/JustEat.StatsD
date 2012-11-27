using System.Linq;
using JustEat.Testing;
using NUnit.Framework;

namespace JustEat.StatsD.Tests
{
	[TestFixture]
	public class WhenBuildingPackets 
	{
		private byte[][] _bytes;


		[TestFixtureSetUp]
		protected void SetBytes()
		{
			_bytes = PacketBuilder.ToMaximumBytePackets(new[] { Enumerable.Repeat("a", 512).ToString() }).ToArray();
		}
		
		[Test]
		public void TheMetricShouldGetSent()
		{
			Assert.That(_bytes[0].Length, Is.InRange(1, 512));
		}
	}



	
}
