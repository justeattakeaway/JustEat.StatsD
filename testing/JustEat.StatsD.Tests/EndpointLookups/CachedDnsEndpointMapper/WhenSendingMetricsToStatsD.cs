using JustEat.StatsD.EndpointLookups;
using JustEat.Testing;
using NUnit.Framework;
using Rhino.Mocks;

namespace JustEat.StatsD.Tests.EndpointLookups.CachedDnsEndpointMapper
{
    [TestFixture]
    public class WhenCacheIsEmpty : BehaviourTest<StatsD.EndpointLookups.CachedDnsEndpointMapper>
    {
        private int _port;
        private string _hostName;

        protected override void Given()
        {
            _port = 0;
            _hostName = "host";
            Mock<IDnsEndpointMapper>().Expect(x => x.GetIpEndPoint(_hostName, _port)).Return(null);
        }

        protected override void When()
        {
            SystemUnderTest.GetIpEndPoint(_hostName, _port);
        }

        [Then]
        public void EndpointIsProvidedByInnerService()
        {
            Mock<IDnsEndpointMapper>().VerifyAllExpectations();
        }
    }
}
