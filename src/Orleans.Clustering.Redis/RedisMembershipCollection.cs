using Orleans.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.Clustering.Redis
{
    [Serializable]
    public class RedisMembershipCollection : IList<RedisMembershipEntry>
    {
        private List<RedisMembershipEntry> list;
        public static readonly TableVersion _tableVersion = new TableVersion(0, "0");

        public RedisMembershipCollection()
        {
            list = new List<RedisMembershipEntry>();
        }

        public RedisMembershipEntry this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        public int Count => list.Count();

        public bool IsReadOnly => false;

        public void Add(RedisMembershipEntry item)
        {
            if (!HasEntry(item))
            {
                list.Add(item);
            }
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(RedisMembershipEntry item)
        {
            return list.Contains(item);
        }

        public void CopyTo(RedisMembershipEntry[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<RedisMembershipEntry> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(RedisMembershipEntry item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, RedisMembershipEntry item)
        {
            list.Insert(index, item);
        }

        public bool Remove(RedisMembershipEntry item)
        {
            return list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }


        public bool HasEntry(RedisMembershipEntry entry)
        {
            var items = list.Where((x) => x.DeploymentId == entry.DeploymentId && x.Address.ToParsableString() == entry.Address.ToParsableString());
            return items.Count() > 0;
        }

        public MembershipTableData ToMembershipTableData()
        {
            try
            {

                var data = list.ToArray().Where((x) => x != null)
                    .Select(x => x.ToMembershipEntryTuple())
                    .ToList();

                return new MembershipTableData(data, _tableVersion);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public MembershipTableData ToMembershipTableData(SiloAddress key)
        {
            var data = list.ToArray().Where((x) => x != null)
                .Select(x => x.ToMembershipEntryTuple())
                .ToList();

            var items = data.TakeWhile((x) => x.Item1.SiloAddress.ToParsableString() == key.ToParsableString()).ToList();
            return new MembershipTableData(items, _tableVersion);
        }

        public bool UpdateIAmAlive(string clusterId, SiloAddress address, DateTime iAmAlivetime)
        {
            bool ret = false;
            string val = iAmAlivetime.ToString();
            var item = list.ToArray().Where((x) => x != null && x.DeploymentId == clusterId && x.ParsableAddress == address.ToParsableString()).First();
            if (item != null)
            {
                item.IAmAliveTime = iAmAlivetime;
                ret = true;
            }

            return ret;
        }


        //public Tuple<MembershipEntry, string> UpdateIAmAlive(string clusterId, SiloAddress address, DateTime iAmAlivetime)
        //{
        //    try
        //    {
        //        string key = String.Format("{0}{1}", clusterId, address.ToParsableString());
        //        string val = iAmAlivetime.ToString();

        //        var data = list.ToArray().Where((x) => x != null)
        //            .Select(x => x.ToMembershipEntryTuple())
        //            .ToList();

        //        var items = data.Where((x) => x.Item1.SiloAddress.ToParsableString() == address.ToParsableString()).ToList();
        //        //var items = data.TakeWhile((x) => x.Item1.SiloAddress.ToParsableString() == address.ToParsableString()).ToList();
        //        if (items == null || items.Count != 1)
        //        {

        //            return null;
        //        }
        //        else
        //        {
        //            items[0].Item1.IAmAliveTime = iAmAlivetime;
        //            return items[0];
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }

        //}

    }
}
