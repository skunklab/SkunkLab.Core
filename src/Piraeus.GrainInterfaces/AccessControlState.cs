using System;

namespace Piraeus.GrainInterfaces
{
    [Serializable]

    public class AccessControlState
    {

        public byte[] Policy { get; set; }
    }
}
