using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using JustBehave;
using JustEat.StatsD.EndpointLookups;

using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;



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
            fixture.Customize(new AutoFakeItEasyCustomization());
            base.CustomizeAutoFixture(fixture);
        }

        protected override void Given()
        {
            _port = 0;
            _hostName = "host";
            _mapper = Fixture.Freeze<IDnsEndpointMapper>();
            _mapper.CallsTo(x => x.GetIPEndPoint(_hostName, _port)).Returns(null);
        }

        protected override void When()
        {
            SystemUnderTest.GetIPEndPoint(_hostName, _port);
        }

        [Then]
        public void EndpointIsProvidedByInnerService()
        {
            _mapper.CallsTo(x => x.GetIPEndPoint(_hostName, _port)).MustHaveHappened();
        }
    }
}
