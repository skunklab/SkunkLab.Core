//using Piraeus.Grains.Notifications;
//using SkunkLab.Storage;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace Piraeus.Grains.Audit
//{
//    public class TableAuditProvider : IAuditor
//    {
//        public TableAuditProvider(string connectionString, string tableName)
//        {
//            storage = TableStorage.CreateSingleton(connectionString);
//            this.tableName = tableName;
//        }

//        private string tableName;
//        private TableStorage storage;

//        public bool CanAudit
//        {
//            get { return !string.IsNullOrEmpty(tableName) && storage != null; }
//        }

//        public async Task WriteAuditRecordAsync(AuditRecord record)
//        {
//            await storage.WriteAsync(tableName, record);
//        }
//    }
//}
