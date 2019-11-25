using SkunkLab.Protocols.Utilities;
using System;
using System.Collections.Generic;

namespace SkunkLab.Clients.Coap
{
    public class CoapClientRequestRegistry
    {
        public CoapClientRequestRegistry()
        {
            container = new Dictionary<string, Action<string, byte[]>>();
        }

        private Dictionary<string, Action<string, byte[]>> container;
        public void Add(string verb, string resourceUriString, Action<string, byte[]> action)
        {
            Uri uri = new Uri(resourceUriString);
            string key = verb.ToUpperInvariant() + uri.ToCanonicalString(false);

            if (!container.ContainsKey(key))
            {
                container.Add(key, action);
            }
        }

        public void Remove(string verb, string resourceUriString)
        {
            Uri uri = new Uri(resourceUriString);
            string key = verb.ToUpperInvariant() + uri.ToCanonicalString(false);
            container.Remove(key);
        }

        public Action<string, byte[]> GetAction(string verb, string resourceUriString)
        {
            Uri uri = new Uri(resourceUriString);
            string key = verb.ToUpperInvariant() + uri.ToCanonicalString(false);

            if (container.ContainsKey(key))
            {
                return container[key];
            }
            else
            {
                return null;
            }
        }

        public void Clear()
        {
            container.Clear();
        }

    }
}
