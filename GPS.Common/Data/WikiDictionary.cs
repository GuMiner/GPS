using System;
using System.Collections.Generic;
using ProtoBuf;

namespace GPS.Common
{
    [ProtoContract]
    public class WikiDictionary
    {
        /// <summary>
        /// Mapping of word -> pages with that word.
        /// </summary>
        [ProtoMember(1)]
        public Dictionary<string, HashSet<Int64>> WordMap { get; set; }

        /// <summary>
        /// Mapping of page ID -> title (primary + secondary, added)
        /// </summary>
        [ProtoMember(2)]
        public Dictionary<Int64, string> TitleMap { get; set; }

        /// <summary>
        /// Mapping of word -> usage frequency
        /// </summary>
        [ProtoMember(3)]
        public Dictionary<string, int> WordFrequencies { get; set; }
    }
}
