using System.Threading.Tasks;

namespace Piraeus.Auditing
{
    public interface IAuditor
    {
        Task WriteAuditRecordAsync(AuditRecord record);
    }
}
