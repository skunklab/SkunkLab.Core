//using Orleans;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace Piraeus.Grains.Audit
//{
//    internal class Auditor : IAuditor
//    {
        
//        public static IAuditor Create(List<IAuditor> list)
//        {
//            if(instance == null)
//            {
//                instance = new Auditor(list);
//            }

//            return instance;
//        }
//        protected Auditor(List<IAuditor> list)
//        {
//            this.list = list;
//        }

//        private List<IAuditor> list;
//        private static IAuditor instance;

//        public bool CanAudit
//        {
//            get
//            {
//                foreach(IAuditor auditor in list)
//                {
//                    if(auditor.CanAudit)
//                    {
//                        return true;
//                    }
//                }

//                return false;
//            }
//        }

//        public async Task WriteAuditRecordAsync(AuditRecord record)
//        {
//            foreach(IAuditor auditor in list)
//            {
//                if(auditor.CanAudit)
//                {
//                    auditor.WriteAuditRecordAsync(record).Ignore();
//                }
//            }

//            await Task.CompletedTask;
//        }
//    }
//}
