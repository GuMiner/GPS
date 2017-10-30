using ProtoBuf;
using System;
using System.Collections.Generic;

namespace GPS.Console
{
    [ProtoContract]
    public class PageIndex
    {
        [ProtoMember(1)]
        public Int64 Id { get; set; }

        [ProtoMember(2)]
        public string PrimaryTitle { get; set; }

        [ProtoMember(3)]
        public List<string> SecondaryTitles { get; set; }
    }
}
