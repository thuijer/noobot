using Noobot.Core.Configuration;
using Noobot.Octopus;

namespace Noobot.Runner
{
    class Config: ConfigurationBase
    {
        public Config()
        {
            UseMiddleware<OctopusMiddleware>(); 
        }
    }
}
