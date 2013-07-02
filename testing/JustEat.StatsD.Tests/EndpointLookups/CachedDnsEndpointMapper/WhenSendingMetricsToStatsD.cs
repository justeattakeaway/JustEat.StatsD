using JustEat.StatsD.EndpointLookups;
using JustEat.Testing;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoRhinoMock;
using Rhino.Mocks;

namespace JustEat.StatsD.Tests.EndpointLookups.CachedDnsEndpointMapper
{
    [TestFixture]
    public class WhenCacheIsEmpty : BehaviourTest<StatsD.EndpointLookups.CachedDnsEndpointMapper>
    {
        private int _port;
        private string _hostName;
        private IDnsEndpointMapper _mapper;

        protected override void CustomizeAutoFixture(Fixture fixture)
        {
            fixture.Customize(new AutoRhinoMockCustomization());
            base.CustomizeAutoFixture(fixture);
        }

        protected override void Given()
        {
            _port = 0;
            _hostName = "host";
            _mapper = Fixture.Freeze<IDnsEndpointMapper>();
            _mapper.Expect(x => x.GetIPEndPoint(_hostName, _port)).Return(null);
        }

        protected override void When()
        {
            SystemUnderTest.GetIPEndPoint(_hostName, _port);
        }

        [Then]
        public void EndpointIsProvidedByInnerService()
        {
            _mapper.VerifyAllExpectations();
        }
    }
}
