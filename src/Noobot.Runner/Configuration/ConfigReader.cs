using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Noobot.Core.Configuration;

namespace Noobot.Runner.Configuration
{
    public class ConfigReader : IConfigReader
    {
        public string SlackApiKey()
        {
            JObject jObject = GetJObject();
            return jObject.Value<string>("slack:apiToken");
        }

        public string OctopusApiKey()
        {
            JObject jObject = GetJObject();
            return jObject.Value<string>("octopus:apiKey");
        }

        public string OctopusApiUrl()
        {
            JObject jObject = GetJObject();
            return jObject.Value<string>("octopus:apiUrl");
        }

        public bool HelpEnabled()
        {
            return true;
        }

        public T GetConfigEntry<T>(string entryName)
        {
            JObject jObject = GetJObject();
            return jObject.Value<T>(entryName);
        }

        private JObject _currentJObject;
        private JObject GetJObject()
        {
            if (_currentJObject == null)
            {
                string assemblyLocation = AssemblyLocation();
                string fileName = Path.Combine(assemblyLocation, @"configuration\config.json");
                string json = File.ReadAllText(fileName);
                _currentJObject = JObject.Parse(json);
            }

            return _currentJObject;
        }

        private string AssemblyLocation()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codebase = new Uri(assembly.CodeBase);
            var path = Path.GetDirectoryName(codebase.LocalPath);
            return path;
        }

        public Dictionary<string, string> GetConfigDictionary(string entryName)
        {
            JObject jObject = GetJObject();
            var jsonDict = jObject.Value<IDictionary<string, JToken>>(entryName);
            return jsonDict.ToDictionary(pair => pair.Key, pair => (string)pair.Value);
        }
    }
}