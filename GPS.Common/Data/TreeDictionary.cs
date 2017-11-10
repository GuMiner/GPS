using ProtoBuf;
using System.Collections.Generic;

namespace GPS.Common.Data
{
    /// <summary>
    /// Represents the word dictionary (mapping string -> int) as a tree structure for enhanced efficiency.
    /// </summary>
    [ProtoContract]
    public class TreeDictionary
    {
        [ProtoMember(1)]
        public List<TreeNode> RootNodes;
    }
}
