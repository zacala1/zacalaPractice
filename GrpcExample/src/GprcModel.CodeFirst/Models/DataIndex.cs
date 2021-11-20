using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GprcModel1.Models
{
    [ProtoContract]
    public class DataIndex
    {
        [ProtoMember(1)]
        public int Index { get; set; }
    }
}
