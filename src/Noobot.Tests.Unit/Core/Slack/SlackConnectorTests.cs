using Noobot.Core;
using Noobot.Core.Logging;
using Noobot.Runner.Configuration;
using Noobot.Tests.Unit.Stubs.MessagingPipeline;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Noobot.Tests.Unit.Core.Slack
{
    [TestFixture]
    public class SlackConnectorTests
    {
        [Test]
        public Task should_connect_as_expected()
        {
            // given
            var configReader = new ConfigReader();
            var containerStub = new NoobotContainerStub();
            var connector = new NoobotCore(configReader, new EmptyLogger(), containerStub);

            // when
            return connector.Connect();
           
            // then

        }
    }
}