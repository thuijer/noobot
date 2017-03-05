using System;
using System.Collections.Generic;
using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;
using System.Text;
using Noobot.Core.Configuration;

namespace Noobot.Octopus
{
    public class OctopusMiddleware : MiddlewareBase
    {
        private string octopusUrl;
        private string octopusApiKey;
        private OctopusVersion octopusVersion;
        public OctopusMiddleware(IMiddleware next, IConfigReader reader) : base(next)
        {
            octopusUrl = reader.GetConfigEntry<string>("octopus:apiUrl");
            octopusApiKey = reader.GetConfigEntry<string>("octopus:apiKey");
            octopusVersion = new OctopusVersion(octopusUrl, octopusApiKey);
            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = new []{ "version" },
                    Description = "Shows version on a given environment",
                    EvaluatorFunc = VersionHandler
                }
            };
        }

        private IEnumerable<ResponseMessage> VersionHandler(IncomingMessage message, string matchedHandle)
        {
            yield return message.ReplyDirectlyToUser("Hold on, I'm asking Octopus");
            StringBuilder builder = new StringBuilder();
            foreach (string s in octopusVersion.Get(""))
                builder.AppendLine(s);

            yield return message.ReplyDirectlyToUser(builder.ToString());
        }
    }
}
