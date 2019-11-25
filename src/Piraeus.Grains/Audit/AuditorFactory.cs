//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Piraeus.Grains.Audit
//{
//    public class AuditorFactory : IAuditorFactory
//    {

//        public static IAuditorFactory Create()
//        {
//            if(instance == null)
//            {
//                instance = new AuditorFactory();
//            }

//            return instance;
//        }

//        protected AuditorFactory()
//        {
//            list = new List<IAuditor>();
//        }

//        private List<IAuditor> list;
//        private static IAuditorFactory instance;

//        public void AddAuditor(IAuditor auditor)
//        {
//            list.Add(auditor);
//        }

//        public void AddTableAuditor(string connectionString, string tableName = "audit")
//        {
//            list.Add(new TableAuditProvider(connectionString, tableName));
//        }

//        public void AddFileAuditor(string path)
//        {
//            list.Add(new FileAuditProvider(path));
//        }

//        public IAuditor GetAuditor()
//        {
//            return Auditor.Create(list);
//        }

//    }
//}
