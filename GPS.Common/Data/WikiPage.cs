using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace GPS.Common
{
    [ProtoContract]
    public class WikiPage
    {
        [ProtoMember(1)]
        public Int32 Id { get; set; }

        [ProtoMember(2)]
        public string PrimaryTitle { get; set; }

        [ProtoMember(3)]
        public List<string> SecondaryTitles { get; set; }

        [ProtoMember(4)]
        public string Content { get; set; }
    }
}
