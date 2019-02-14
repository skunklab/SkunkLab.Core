using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.Clustering.Redis
{
    [Serializable]
    public class RedisMembershipEntry
    {
        public RedisMembershipEntry()
        {
        }

        public static RedisMembershipEntry Create(string deploymentId, MembershipEntry entry, String etag)
        {

            var ret = new RedisMembershipEntry
            {
                DeploymentId = deploymentId,
                Address = entry.SiloAddress,
                IAmAliveTime = entry.IAmAliveTime,
                LastIndex = Convert.ToUInt64(etag),
                Hostname = entry.HostName,
                ProxyPort = entry.ProxyPort,
                StartTime = entry.StartTime,
                Status = entry.Status,
                SiloName = entry.SiloName,
                UpdateZone = entry.UpdateZone,
                FaultZone = entry.FaultZone,
                SuspectingSilos = entry.SuspectTimes?.Select(silo => new SuspectingSilo { Id = silo.Item1.ToParsableString(), Time = silo.Item2 }).ToList()
            };

            return ret;
        }

        //[NonSerialized]
        //private string deploymentId;

        //[NonSerialized]
        //private SiloAddress siloAddress;

        //[NonSerialized]
        //private DateTime? iAmAliveTime;

        //[NonSerialized]
        //private ulong? lastIndex;

        private string parsableAddress;


        public string DeploymentId { get; set; }

        internal SiloAddress Address
        {
            get
            {
                return SiloAddress.FromParsableString(parsableAddress);
            }
            set
            {
                ParsableAddress = value.ToParsableString();
            }
        }

        public DateTime? IAmAliveTime { get; set; }

        public ulong? LastIndex { get; set; }

        public int UpdateZone { get; set; }

        public int FaultZone { get; set; }

        public string ParsableAddress
        {
            get
            {
                return parsableAddress;
            }
            set
            {
                parsableAddress = value;
            }
        }

        public String Hostname { get; set; }
        
        public Int32 ProxyPort { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public SiloStatus Status { get; set; }
        
        public String SiloName { get; set; }
        
        public List<SuspectingSilo> SuspectingSilos { get; set; }

        public Tuple<MembershipEntry, String> ToMembershipEntryTuple()
        {
            MembershipEntry entry = new MembershipEntry()
            {
                HostName = this.Hostname,
                IAmAliveTime = this.IAmAliveTime.HasValue ? this.IAmAliveTime.Value : DateTime.UtcNow,
                ProxyPort = this.ProxyPort,
                RoleName = String.Empty,
                SiloAddress = this.Address,
                SiloName = this.SiloName,
                StartTime = this.StartTime,
                Status = this.Status,
                UpdateZone = this.UpdateZone,
                FaultZone = this.FaultZone,
                SuspectTimes = this.SuspectingSilos?.Select(silo => new Tuple<SiloAddress, DateTime>(SiloAddress.FromParsableString(silo.Id), silo.Time.Value)).ToList(),
            };

            return new Tuple<MembershipEntry, String>(entry, this.LastIndex.ToString());
        }

        public Tuple<MembershipEntry, String> ToMembershipEntryTuple(SiloAddress address)
        {
            MembershipEntry entry = new MembershipEntry()
            {
                HostName = this.Hostname,
                IAmAliveTime = this.IAmAliveTime.HasValue ? this.IAmAliveTime.Value : DateTime.UtcNow,
                ProxyPort = this.ProxyPort,
                RoleName = String.Empty,
                SiloAddress = address,
                SiloName = this.SiloName,
                StartTime = this.StartTime,
                Status = this.Status,
                UpdateZone = 0,
                FaultZone = 0,
                SuspectTimes = this.SuspectingSilos?.Select(silo => new Tuple<SiloAddress, DateTime>(SiloAddress.FromParsableString(silo.Id), silo.Time.Value)).ToList(),
            };

            return new Tuple<MembershipEntry, String>(entry, this.LastIndex.ToString());
        }

    }

    [Serializable]
    public class SuspectingSilo
    {
        [NonSerialized]
        private DateTime? time;

        public String Id { get; set; }

        public DateTime? Time { get { return time; } set { time = value; } }
    }

}
