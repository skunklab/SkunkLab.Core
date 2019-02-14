using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.Silo
{
    public interface IPiraeusHost
    {
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
