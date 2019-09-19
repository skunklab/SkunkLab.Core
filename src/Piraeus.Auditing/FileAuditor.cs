using SkunkLab.Storage;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Auditing
{
    public class FileAuditor : IAuditor
    {
        public FileAuditor(string path)
        {
            storage = LocalFileStorage.Create();
            this.path = path;
        }

        private LocalFileStorage storage;
        private string path;

        public async Task WriteAuditRecordAsync(AuditRecord record)
        {
            byte[] source = Encoding.UTF8.GetBytes(record.ConvertToCsv());
            storage.AppendFileAsync(path, source, 100000).IgnoreException();
            await Task.CompletedTask;
        }

        public async Task UpdateAuditRecordAsync(AuditRecord record)
        {
            byte[] source = Encoding.UTF8.GetBytes(record.ConvertToCsv());
            storage.AppendFileAsync(path, source, 100000).IgnoreException();
            await Task.CompletedTask;
        }
    }
}
