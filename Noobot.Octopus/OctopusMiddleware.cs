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
        private string url;
        private string apiKey;
        private Dictionary<string, string> tenants;
        private OctopusVersion octopusVersion;

        public OctopusMiddleware(IMiddleware next, IConfigReader reader) : base(next)
        {
            url = reader.GetConfigEntry<string>("octopus:apiUrl");
            apiKey = reader.GetConfigEntry<string>("octopus:apiKey");
            tenants = reader.GetConfigDictionary("octopus:tenants");
            octopusVersion = new OctopusVersion(url, apiKey, tenants);
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
            yield return message.ReplyDirectlyToUser($"Hold on {message.Username}, I'm asking Octopus");

            var argument = message.TargetedText.Substring(matchedHandle.Length).Trim();
            StringBuilder builder = new StringBuilder("```");
            foreach (string s in octopusVersion.Get(argument)) 
                builder.AppendLine(s);
            builder.Append("```");
            yield return message.ReplyDirectlyToUser(builder.ToString());
        }
    }
}
