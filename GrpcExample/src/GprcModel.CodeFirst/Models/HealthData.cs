using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GprcModel1.Models
{
    [ProtoContract]
    public class HealthData
    {
        [ProtoMember(1)]
        public string Index { get; set; }
        [ProtoMember(2, Options = MemberSerializationOptions.Required)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public ulong HeartBeat { get; set; }
    }
}
