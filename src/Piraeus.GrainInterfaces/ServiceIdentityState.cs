using System;
using System.Collections.Generic;

namespace Piraeus.GrainInterfaces
{
    [Serializable]
    public class ServiceIdentityState
    {
        public byte[] Certificate { get; set; }

        public List<KeyValuePair<string, string>> Claims { get; set; }
    }
}
