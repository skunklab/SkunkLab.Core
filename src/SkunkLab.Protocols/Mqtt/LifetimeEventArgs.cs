using System;

namespace SkunkLab.Protocols.Mqtt
{
    public class LifetimeEventArgs : EventArgs
    {
        public LifetimeEventArgs(ushort[] ids)
        {
            Ids = ids;
        }

        public ushort[] Ids { get; internal set; }
    }
}
