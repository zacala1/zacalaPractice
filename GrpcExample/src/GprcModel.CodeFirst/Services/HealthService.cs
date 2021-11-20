using GprcModel1.Models;
using ProtoBuf.Grpc.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GprcModel1.Services
{
    [Service]
    public interface HealthService
    {
        ValueTask<HealthData> GetHealthDataAsync(DataIndex id);
    }
}
