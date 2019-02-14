using SkunkLab.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Auditing
{
    public class AzureTableAuditor : IAuditor
    {
        public AzureTableAuditor(string connectionString, string tableName = "useraudit", long? maxBufferSize = null, int? defaultBufferSize = null)
        {
            if (!maxBufferSize.HasValue)
            {
                storage = TableStorage.CreateSingleton(connectionString);
            }
            else
            {
                storage = TableStorage.CreateSingleton(connectionString, maxBufferSize.Value, defaultBufferSize.Value);
            }

            this.tableName = tableName;
        }

        private string tableName;
        private TableStorage storage;


        public async Task WriteAuditRecordAsync(AuditRecord record)
        {
            storage.WriteAsync(tableName, record).IgnoreException();
            await Task.CompletedTask;
        }
    }
}
