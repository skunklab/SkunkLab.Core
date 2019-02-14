//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using Piraeus.Grains.Notifications;
//using SkunkLab.Storage;

//namespace Piraeus.Grains.Audit
//{
//    public class FileAuditProvider : IAuditor
//    {
//        public FileAuditProvider(string path)
//        {
//            storage = LocalFileStorage.Create();
//            this.path = path;            
//        }

//        private LocalFileStorage storage;
//        private string path;
//        public bool CanAudit
//        {
//            get { return !string.IsNullOrEmpty(path) && storage != null; }
//        }

//        public async Task WriteAuditRecordAsync(AuditRecord record)
//        {
//            string recordString = record.ConvertToCsv();
//            await storage.AppendFileAsync("path", Encoding.UTF8.GetBytes(recordString));
//        }
//    }
//}
