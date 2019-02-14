using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Auditing
{
    public interface IAuditFactory
    {
        void Add(IAuditor auditor, AuditType type);

        IAuditor GetAuditor(AuditType type);
    }
}
