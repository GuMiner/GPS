using ProtoBuf;
using System.Collections.Generic;

namespace GPS.Common.Data
{
    public class TreeNode
    {
        [ProtoMember(1)]
        public char Character { get; set; }

        /// <summary>
        /// Total number of words at this point in the tree. If <see cref="Children"/> is null or empty, this is the total number of occurances of this word in Wikipedia.
        /// </summary>
        [ProtoMember(2)]
        public int Count {get; set;}

        [ProtoMember(3)]
        public List<TreeNode> Children { get; set; }
    }
}
