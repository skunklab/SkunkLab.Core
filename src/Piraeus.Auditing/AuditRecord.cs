using Microsoft.WindowsAzure.Storage.Table;

namespace Piraeus.Auditing
{

    public abstract class AuditRecord : TableEntity
    {
        public abstract string ConvertToJson();

        public abstract string ConvertToCsv();
    }

}
