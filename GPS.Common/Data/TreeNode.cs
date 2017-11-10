using ProtoBuf;
using System;
using System.Collections.Generic;

namespace GPS.Common.Data
{
    [ProtoContract]
    public class TreeNode
    {
        [ProtoMember(1)]
        public char Character { get; set; }

        /// <summary>
        /// Total number of words at this point in the tree.
        /// </summary>
        [ProtoMember(2)]
        public int Count {get; set;}

        /// <summary>
        /// Total number of words that end with this character. Will equal <see cref="Count"/> if <see cref="Children"/> is empty.
        /// </summary>
        [ProtoMember(3)]
        public int TerminationCount { get; set; }

        [ProtoMember(4)]
        public List<TreeNode> Children { get; set; }
    }
}
