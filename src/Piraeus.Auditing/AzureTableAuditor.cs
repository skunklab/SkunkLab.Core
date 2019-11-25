using SkunkLab.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piraeus.Auditing
{
    public class AzureTableAuditor : IAuditor
    {
        public AzureTableAuditor(string connectionString, string tableName, long? maxBufferSize = null, int? defaultBufferSize = null)
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

        public async Task UpdateAuditRecordAsync(AuditRecord record)
        {
            UserAuditRecord userRecord = record as UserAuditRecord;
            if (userRecord != null)
            {
                List<UserAuditRecord> list = await storage.ReadAsync<UserAuditRecord>(tableName, record.PartitionKey, record.RowKey);
                if (list?.Count == 1)
                {
                    UserAuditRecord updateRecord = list[0];
                    updateRecord.LogoutTime = userRecord.LogoutTime;
                    storage.WriteAsync(tableName, updateRecord).IgnoreException();
                }
            }

        }
    }
}
